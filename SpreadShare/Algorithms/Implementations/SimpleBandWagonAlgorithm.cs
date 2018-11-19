using System;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices;
using SpreadShare.ExchangeServices.Providers;
using SpreadShare.Models;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.Algorithms.Implementations
{
    /// <summary>
    /// Simple bandwagon algorithm, works as follows.
    /// Starting Condition: 100% holding base currency
    /// Check most risen coin against base currency,
    /// if it performs better that a minimal percentage,
    /// fully change position to that asset and hold for the holdingTime before checking again.
    /// If their is no winner, remain in baseCurrency and check again after waitTime.
    /// </summary>
    internal class SimpleBandWagonAlgorithm : BaseAlgorithm
    {
        /// <inheritdoc />
        public override Type GetSettingsType => typeof(SimpleBandWagonAlgorithmSettings);

        /// <inheritdoc />
        public override ResponseObject Start(
            AlgorithmSettings settings,
            ExchangeProvidersContainer container)
        {
            var stateManager = new StateManager<SimpleBandWagonAlgorithmSettings>(
                settings as SimpleBandWagonAlgorithmSettings,
                new EntryState(),
                container);

            return new ResponseObject(ResponseCode.Success);
        }

        /// <summary>
        /// Starting state of the algorithm
        /// </summary>
       private class EntryState : State<SimpleBandWagonAlgorithmSettings>
        {
            public override State<SimpleBandWagonAlgorithmSettings> OnMarketCondition(DataProvider data)
            {
                var query = data.GetTopPerformance(AlgorithmSettings.ActiveTradingPairs, AlgorithmSettings.CheckTime).Data;
                if (query.Item2 < 0.98M)
                {
                    Logger.LogCritical("Panic detected, let's go");
                    return new BuyState();
                }

                return new NothingState<SimpleBandWagonAlgorithmSettings>();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
            }
        }

        /// <summary>
        /// Starting state of the algorithm
        /// </summary>
        private class BuyState : State<SimpleBandWagonAlgorithmSettings>
        {
            private OrderUpdate _buy;

            public override State<SimpleBandWagonAlgorithmSettings> OnTimerElapsed()
            {
                Logger.LogCritical("Hold time has exceeded, selling.");
                return new SellState();
            }

            public override State<SimpleBandWagonAlgorithmSettings> OnMarketCondition(DataProvider data)
            {
                decimal price = data.GetCurrentPriceLastTrade(TradingPair.Parse("EOSETH")).Data;
                if (price > _buy.SetPrice * 1.005M)
                {
                    Logger.LogInformation("Price has increased, selling");
                    return new SellState();
                }

                return new NothingState<SimpleBandWagonAlgorithmSettings>();
            }

            /// <inheritdoc />
            protected override void Run(TradingProvider trading, DataProvider data)
            {
               Logger.LogInformation($"Portfolio is {trading.GetPortfolio().ToJson()}");
               _buy = trading.PlaceFullMarketOrder(TradingPair.Parse("EOSETH"), OrderSide.Buy).Data;
               Logger.LogInformation($"Portfolio is now {trading.GetPortfolio().ToJson()}");
               SetTimer(TimeSpan.FromHours(AlgorithmSettings.HoldTime));
            }
        }

        private class SellState : State<SimpleBandWagonAlgorithmSettings>
        {
            public override State<SimpleBandWagonAlgorithmSettings> OnTimerElapsed()
            {
                return new EntryState();
            }

            protected override void Run(TradingProvider trading, DataProvider data)
            {
               Logger.LogInformation("Selling it all, hybernating");
               trading.PlaceFullMarketOrder(TradingPair.Parse("EOSETH"), OrderSide.Sell);
               SetTimer(TimeSpan.FromHours(2));
            }
        }

        /*
        /// <summary>
        /// Checks if the winner is not already the majority share of the portfolio.
        /// </summary>
        private class CheckPositionValidityState : State<SimpleBandWagonAlgorithmSettings>
        {
            /// <inheritdoc />
            protected override void Run()
            {
                // Retrieve global settings
                Currency baseSymbol = AlgorithmSettings.BaseCurrency;
                uint checkTime = AlgorithmSettings.CheckTime;
                var activeTradingPairs = AlgorithmSettings.ActiveTradingPairs;

                // Try to get to top performer, if not try state again after 10 seconds
                var winnerQuery = DataProvider.GetTopPerformance(activeTradingPairs, checkTime, DateTime.Now);
                if (!winnerQuery.Success)
                {
                    Logger.LogError($"Could not get top performer!\n{winnerQuery}\ntrying again after 1 minute");
                    SwitchState(new TryAfterWaitState(1, new CheckPositionValidityState()));
                    return;
                }

                // Calculate and show the percentage of increase
                var winnerPair = winnerQuery.Data.Item1;
                decimal deltaPercentage = (winnerQuery.Data.Item2 * 100) - 100;
                Logger.LogInformation($"Top performer from the past {checkTime} hours is {winnerPair} | {deltaPercentage}%");

                // Filter wether this 'winner' is gained enough growth to undertake action, otherwise just got the WaitHolding state again.
                if (deltaPercentage < AlgorithmSettings.MinimalGrowthPercentage)
                {
                    Logger.LogInformation($"Growth is less than {AlgorithmSettings.MinimalGrowthPercentage}%, disregard.");
                    SwitchState(new RevertToBaseState());
                    return;
                }

                // Retrieve all the assets to determine if perhaps the desired asset is already a majority share, in which case we do nothing.
                var assetsQuery = TradingProvider.GetPortfolio();
                if (!assetsQuery.Success)
                {
                    Logger.LogError($"Could not get portfolio!\n{assetsQuery}\ntrying again after 1 minute");
                    SwitchState(new TryAfterWaitState(1, new CheckPositionValidityState()));
                    return;
                }

                var assets = assetsQuery.Data.GetAllFreeBalances();

                // 1. Map the assets values to their respective pairs using baseSymbol values
                // 2. Order by this newgained value, making the last element the most valuable.
                var sorted = assets.ToArray().Select(x =>
                {
                    TradingPair pair;
                    try
                    {
                        pair = TradingPair.Parse($"{x.Symbol}{baseSymbol}");
                    }
                    catch
                    {
                        return new AssetValue(x.Symbol, 0);
                    }
                    var query = DataProvider.GetCurrentPriceTopBid(pair);

                    // Use a value of zero for assets whose price retrievals fail.
                    return query.Success ? new AssetValue(x.Symbol, x.Amount * query.Data) : new AssetValue(x.Symbol, 0);
                }).OrderBy(x => x.Amount);
                Logger.LogInformation($"Most valuable asset in portfolio: {sorted.Last().Symbol}");

                // Construct the most valueble asset as a currency
                Currency majorityAsset = sorted.Last().Symbol;

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
        private class RevertToBaseState : State<SimpleBandWagonAlgorithmSettings>
        {
            /// <inheritdoc />
            protected override void Run()
            {
                // Retrieve globals from the settings.
                Currency baseSymbol = AlgorithmSettings.BaseCurrency;
                decimal valueMinimum = AlgorithmSettings.MinimalRevertValue;

                // Retrieve the portfolio, using a fallback in case of failure.
                var assetsQuery = TradingProvider.GetPortfolio();
                if (!assetsQuery.Success)
                {
                    Logger.LogWarning("Could not get portfolio, going idle for 1 minute, then try again.");
                    SwitchState(new TryAfterWaitState(1, new RevertToBaseState()));
                    return;
                }

                // Iterate through all the assets
                var assets = assetsQuery.Data.GetAllFreeBalances();
                foreach (var asset in assets)
                {
                    // Skip the base currency itself (ETHETH e.d. makes no sense)
                    if (asset.Symbol == baseSymbol)
                    {
                        continue;
                    }

                    // Try to get a valid pair against the base assets
                    TradingPair pair;
                    try
                    {
                        pair = TradingPair.Parse($"{asset.Symbol}{baseSymbol}");
                    }
                    catch (Exception)
                    {
                        Logger.LogWarning($"{asset.Symbol}{baseSymbol} could not be parsed, is this asset listed on the exhange?");
                        continue;
                    }

                    // Get the price of pair (thus in terms of baseCurrency)
                    var priceQuery = DataProvider.GetCurrentPriceTopBid(pair);

                    // In case of failure, just skip
                    if (!priceQuery.Success)
                    {
                        Logger.LogWarning($"Could not get price estimate for {pair}");
                        continue;
                    }

                    decimal price = priceQuery.Data;

                    // Check if the eth value of the asset exceeds the minimum to be consired relevant
                    decimal value = price * asset.Amount;
                    if (value >= valueMinimum)
                    {
                        Logger.LogInformation($"Reverting for {pair}");
                        var orderQuery = TradingProvider.PlaceFullMarketOrder(pair, OrderSide.Sell);
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
        private class BuyState : State<SimpleBandWagonAlgorithmSettings>
        {
            /// <inheritdoc />
            protected override void Run()
            {
                // Retrieve globals from the settings.
                uint checkTime = AlgorithmSettings.CheckTime;
                var activeTradingPairs = AlgorithmSettings.ActiveTradingPairs;

                // Try to retrieve the top performer, using a tryAfterWait fallback in case of failure.
                Logger.LogInformation($"Looking for the top performer from the previous {checkTime} hours");
                var query = DataProvider.GetTopPerformance(activeTradingPairs, checkTime, DateTime.Now);
                if (query.Success)
                {
                    Logger.LogInformation($"Top performer is {query.Data.Item1}");
                }
                else
                {
                    Logger.LogWarning($"Could not fetch top performer, {query}\nRetrying state after 1 minute");
                    SwitchState(new TryAfterWaitState(1, new BuyState()));
                    return;
                }

                // Calculate and show the percentage of increase
                var winnerPair = query.Data.Item1;
                decimal deltaPercentage = (query.Data.Item2 * 100) - 100;
                Logger.LogInformation($"Top performer from the past {checkTime} hours is {winnerPair} | {deltaPercentage}%");

                // Filter wether this 'winner' is gained enough growth to undertake action, otherwise just got the WaitHolding state again.
                if (deltaPercentage < AlgorithmSettings.MinimalGrowthPercentage)
                {
                    Logger.LogInformation($"Growth is less than {AlgorithmSettings.MinimalGrowthPercentage}%, disregard.");
                    SwitchState(new WaitHoldingState());
                    return;
                }

                // Place an order for the selected winner and goin into holding (again using a tryAfterWait fallback option)
                var response = TradingProvider.PlaceFullMarketOrder(query.Data.Item1, OrderSide.Buy);
                if (response.Success)
                {
                    SwitchState(new WaitHoldingState());
                }
                else
                {
                    Logger.LogError($"Order has failed, retrying state in 1 minute\n{response}");
                    SwitchState(new TryAfterWaitState(1, new BuyState()));
                }
            }
        }

        /// <summary>
        /// What as many hours as the holdTime dictactes and then proceed to checking the position again.
        /// </summary>
        private class WaitHoldingState : State<SimpleBandWagonAlgorithmSettings>
        {
            /// <inheritdoc />
            public override ResponseObject OnTimer()
            {
                Logger.LogInformation($"Waking up! ({DateTime.UtcNow})");

                // First step after holding is verifying the current position.
                SwitchState(new CheckPositionValidityState());

                // TODO: Will this return statement fire before or after CheckPositionValidity has occured?
                return new ResponseObject(ResponseCode.Success);
            }

            /// <inheritdoc />
            protected override void Run()
            {
                Logger.LogInformation($"Going to sleep for {AlgorithmSettings.HoldTime} hours ({DateTime.UtcNow})");

                SetTimer(60 * AlgorithmSettings.HoldTime);
            }
        }

        /// <summary>
        /// Helper state that enables 'try again after wait' solutions
        /// when exceptions pop up.
        /// </summary>
        private class TryAfterWaitState : State<SimpleBandWagonAlgorithmSettings>
        {
            private readonly uint _idleTime;
            private readonly State<SimpleBandWagonAlgorithmSettings> _callback;

            /// <summary>
            /// Initializes a new instance of the <see cref="TryAfterWaitState"/> class.
            /// </summary>
            /// <param name="idleTime">The number of minutes to wait.</param>
            /// <param name="callback">The state to which to return after the idleTime,
            /// This will likely be a new instance of the state from which this state is
            /// created.</param>
            public TryAfterWaitState(uint idleTime, State<SimpleBandWagonAlgorithmSettings> callback)
            {
                _idleTime = idleTime;
                _callback = callback;
            }

            /// <inheritdoc />
            public override ResponseObject OnTimer()
            {
                SwitchState(_callback);
                return new ResponseObject(ResponseCode.Success);
            }

            /// <inheritdoc />
            protected override void Run()
            {
                SetTimer(_idleTime);
            }
        }*/
    }
}