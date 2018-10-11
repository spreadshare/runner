using System;
using System.Linq;
using Binance.Net.Objects;
using Microsoft.Extensions.Logging;
using SpreadShare.BinanceServices;
using SpreadShare.Models;
using SpreadShare.SupportServices;

namespace SpreadShare.Strategy.Implementations
{
    /// <summary>
    /// Simple bandwagon strategy, works as follows.
    /// Starting Condition: 100% holding base currency
    /// Check most risen coin against base currency,
    /// if it performs better that a minimal percentage,
    /// fully change position to that asset and hold for the holdingTime before checking again.
    /// If their is no winner, remain in baseCurrency and check again after waitTime.
    /// </summary>
    internal class SimpleBandWagonStrategy : BaseStrategy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleBandWagonStrategy"/> class.
        /// </summary>
        /// <param name="loggerFactory">Provided logger creating capabilities</param>
        /// <param name="tradingService">Provides trading capabilities</param>
        /// <param name="userService">Provides user data fetching capabilities</param>
        /// <param name="settingsService">Provides access to global settings</param>
        public SimpleBandWagonStrategy(
            ILoggerFactory loggerFactory,
            ITradingService tradingService,
            IUserService userService,
            ISettingsService settingsService)
            : base(loggerFactory, tradingService, userService, settingsService)
        {
        }

        /// <inheritdoc />
        public override State GetInitialState() => new EntryState();

        /// <summary>
        /// Starting state of the strategy
        /// </summary>
        // TODO: This state seems entirely unnecessary?
        // TODO^: Little effort and give a nice confirmation that all has started well.
        private class EntryState : State
        {
            /// <inheritdoc />
            protected override void Run()
            {
                Logger.LogInformation("Started the simple bandwagon strategy");
                SwitchState(new CheckPositionValidityState());
            }
        }

        /// <summary>
        /// Checks if the winner is not already the majority share of the portfolio.
        /// </summary>
        private class CheckPositionValidityState : State
        {
            /// <inheritdoc />
            protected override void Run()
            {
                // Retrieve global settings
                Currency baseSymbol = SettingsService.SimpleBandWagonStrategySettings.BaseCurrency;
                uint checkTime = SettingsService.SimpleBandWagonStrategySettings.CheckTime;

                // Try to get to top performer, if not try state again after 10 seconds
                var winnerQuery = TradingService.GetTopPerformance(checkTime, DateTime.Now);
                if (!winnerQuery.Success)
                {
                    Logger.LogError($"Could not get top performer!\n{winnerQuery}\ntrying again after 10 seconds");
                    SwitchState(new TryAfterWaitState(10000, new CheckPositionValidityState()));
                    return;
                }

                // Calculate and show the percentage of increase
                var winnerPair = winnerQuery.Data.Item1;
                decimal deltaPercentage = (winnerQuery.Data.Item2 * 100) - 100;
                Logger.LogInformation($"Top performer from the past {checkTime} hours is {winnerPair} | {deltaPercentage}%");

                // Filter wether this 'winner' is gained enough growth to undertake action, otherwise just got the WaitHolding state again.
                if (deltaPercentage < SettingsService.SimpleBandWagonStrategySettings.MinimalGrowthPercentage)
                {
                    Logger.LogInformation($"Growth is less than {SettingsService.SimpleBandWagonStrategySettings.MinimalGrowthPercentage}%, disregard.");
                    SwitchState(new RevertToBaseState());
                    return;
                }

                // Retrieve all the assets to determine if perhaps the desired asset is already a majority share, in which case we do nothing.
                var assetsQuery = UserService.GetPortfolio();
                if (!assetsQuery.Success)
                {
                    Logger.LogError($"Could not get portfolio!\n{assetsQuery}\ntrying again after 10 seconds");
                    SwitchState(new TryAfterWaitState(10000, new CheckPositionValidityState()));
                    return;
                }

                var assets = assetsQuery.Data.GetAllFreeBalances();

                // 1. Map the assets values to their respective baseSymbol values
                // 2. Order by this newgained value, making the last element the most valuable.
                var sorted = assets.ToArray().Select(x =>
                    {
                        CurrencyPair pair;
                        try
                        {
                            pair = CurrencyPair.Parse($"{x.Symbol}{baseSymbol}");
                        }
                        catch
                        {
                            return new AssetValue(x.Symbol, 0);
                        }
                        var query = TradingService.GetCurrentPrice(pair);

                        // Use a value of zero for assets whose price retrievals fail.
                        return query.Success ? new AssetValue(x.Symbol, x.Value * query.Data) : new AssetValue(x.Symbol, 0);
                    }).OrderBy(x => x.Value);
                Logger.LogInformation($"Most valuable asset in portfolio: {sorted.Last().Symbol}");

                // Construct the most valueble asset as a currency
                Currency majorityAsset = new Currency(sorted.Last().Symbol);

                // Verify if this asset was also the top performer (winner)
                if (majorityAsset == winnerPair.Left)
                {
                    Logger.LogInformation($"Already in the possesion of the winner: {winnerPair}");
                    SwitchState(new WaitHoldingState());
                }
                else
                {
                    SwitchState(new RevertToBaseState());
                }
            }
        }

        /// <summary>
        /// Trades in all relevant assets for the base currency.
        /// </summary>
        private class RevertToBaseState : State
        {
            /// <inheritdoc />
            protected override void Run()
            {
                // Retrieve globals from the settings.
                Currency baseSymbol = SettingsService.SimpleBandWagonStrategySettings.BaseCurrency;
                decimal valueMinimum = SettingsService.SimpleBandWagonStrategySettings.MinimalRevertValue;

                // Retrieve the portfolio, using a fallback in case of failure.
                var assetsQuery = UserService.GetPortfolio();
                if (!assetsQuery.Success)
                {
                    Logger.LogWarning("Could not get portfolio, going idle for 10 seconds, then try again.");
                    SwitchState(new TryAfterWaitState(10000, new RevertToBaseState()));
                    return;
                }

                // Iterate through all the assets
                var assets = assetsQuery.Data.GetAllFreeBalances();
                foreach (var asset in assets)
                {
                    // Skip the base currency itself (ETHETH e.d. makes no sense)
                    if (asset.Symbol == baseSymbol.ToString())
                    {
                        continue;
                    }

                    // Try to get a valid pair against the base assets
                    CurrencyPair pair;
                    try
                    {
                        pair = CurrencyPair.Parse($"{asset.Symbol}{baseSymbol}");
                    }
                    catch (Exception)
                    {
                        Logger.LogWarning($"{asset.Symbol}{baseSymbol} could not be parsed, is this asset listed on the exhange?");
                        continue;
                    }

                    // Get the price of pair (thus in terms of baseCurrency)
                    var priceQuery = TradingService.GetCurrentPrice(pair);

                    // In case of failure, just skip
                    if (!priceQuery.Success)
                    {
                        Logger.LogWarning($"Could not get price estimate for {pair}");
                        continue;
                    }

                    decimal price = priceQuery.Data;

                    // Check if the eth value of the asset exceeds the minimum to be consired relevant
                    decimal value = price * asset.Value;
                    if (value >= valueMinimum)
                    {
                        Logger.LogInformation($"Reverting for {pair}");
                        var orderQuery = TradingService.PlaceFullMarketOrder(pair, OrderSide.Sell);
                        if (!orderQuery.Success)
                        {
                            Logger.LogWarning($"Reverting for {pair} failed! Is this pair trading on the exchange?");
                        }
                    }
                    else
                    {
                        Logger.LogInformation($"{pair} value not relevant ({value}{baseSymbol})");
                    }
                }

                Logger.LogInformation("Reverting to base succeeded.");
                SwitchState(new BuyState());
            }
        }

        /// <summary>
        /// Obtain the top performing coin
        /// (This will execute a trade even if the coin is already the majority share,
        /// consider to run the CheckPositionValidityState first.)
        /// </summary>
        private class BuyState : State
        {
            /// <inheritdoc />
            protected override void Run()
            {
                // Retrieve globals from the settings.
                uint checkTime = SettingsService.SimpleBandWagonStrategySettings.CheckTime;

                // Try to retrieve the top performer, using a tryAfterWait fallback in case of failure.
                Logger.LogInformation($"Looking for the top performer from the previous {checkTime} hours");
                var query = TradingService.GetTopPerformance(checkTime, DateTime.Now);
                if (query.Success)
                {
                    Logger.LogInformation($"Top performer is {query.Data.Item1}");
                }
                else
                {
                    Logger.LogWarning($"Could not fetch top performer, {query}\nRetrying state after 10 seconds");
                    SwitchState(new TryAfterWaitState(10000, new BuyState()));
                    return;
                }

                // Calculate and show the percentage of increase
                var winnerPair = query.Data.Item1;
                decimal deltaPercentage = (query.Data.Item2 * 100) - 100;
                Logger.LogInformation($"Top performer from the past {checkTime} hours is {winnerPair} | {deltaPercentage}%");

                // Filter wether this 'winner' is gained enough growth to undertake action, otherwise just got the WaitHolding state again.
                if (deltaPercentage < SettingsService.SimpleBandWagonStrategySettings.MinimalGrowthPercentage)
                {
                    Logger.LogInformation($"Growth is less than {SettingsService.SimpleBandWagonStrategySettings.MinimalGrowthPercentage}%, disregard.");
                    SwitchState(new WaitHoldingState());
                    return;
                }

                // Place an order for the selected winner and goin into holding (again using a tryAfterWait fallback option)
                var response = TradingService.PlaceFullMarketOrder(query.Data.Item1, OrderSide.Buy);
                if (response.Success)
                {
                    SwitchState(new WaitHoldingState());
                }
                else
                {
                    Logger.LogError($"Order has failed, retrying state in 10 seconds\n{response}");
                    SwitchState(new TryAfterWaitState(10000, new BuyState()));
                }
            }
        }

        /// <summary>
        /// What as many hours as the holdTime dictactes and then proceed to checking the position again.
        /// </summary>
        private class WaitHoldingState : State
        {
            /// <inheritdoc />
            public override ResponseObject OnTimer()
            {
                Logger.LogInformation($"Waking up! ({DateTime.UtcNow})");

                // First step after holding is verifying the current position.
                SwitchState(new CheckPositionValidityState());

                // TODO: Will this return statement fire before or after CheckPositionValidity has occured?
                return new ResponseObject(ResponseCodes.Success);
            }

            /// <inheritdoc />
            protected override void Run()
            {
                Logger.LogInformation($"Going to sleep for {SettingsService.SimpleBandWagonStrategySettings.HoldTime} hours ({DateTime.UtcNow})");

                // 1000 ms / s
                // 3600 s / h
                SetTimer(1000 * 3600 * SettingsService.SimpleBandWagonStrategySettings.HoldTime);
            }
        }

        /// <summary>
        /// Helper state that enables 'try again after wait' solutions
        /// when exceptions pop up.
        /// </summary>
        private class TryAfterWaitState : State
        {
            private readonly uint _idleTime;
            private readonly State _callback;

            /// <summary>
            /// Initializes a new instance of the <see cref="TryAfterWaitState"/> class.
            /// </summary>
            /// <param name="idleTime">The amount of milliseconds to wait</param>
            /// <param name="callback">The state to which to return after the idleTime,
            /// This will likely be a new instance of the state from which this state is
            /// created.</param>
            public TryAfterWaitState(uint idleTime, State callback)
            {
                _idleTime = idleTime;
                _callback = callback;
            }

            /// <inheritdoc />
            public override ResponseObject OnTimer()
            {
                SwitchState(_callback);
                return new ResponseObject(ResponseCodes.Success);
            }

            /// <inheritdoc />
            protected override void Run()
            {
                SetTimer(_idleTime);
            }
        }
    }
}
