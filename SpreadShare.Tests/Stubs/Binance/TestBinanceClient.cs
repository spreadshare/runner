using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Binance.Net;
using Binance.Net.Interfaces;
using Binance.Net.Objects;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using SpreadShare.ExchangeServices.ProvidersBinance;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.Configuration;
using OrderSide = Binance.Net.Objects.OrderSide;

namespace SpreadShare.Tests.Stubs.Binance
{
    internal class TestBinanceClient : IBinanceClient, IDisposable
    {
        private readonly BinanceClient _implementation;
        private readonly Random _orderIdGenerator;
        private readonly TestBinanceCommunicationsService _parent;

        public TestBinanceClient(TestBinanceCommunicationsService parent)
        {
            _parent = parent;
            _orderIdGenerator = new Random();
            _implementation = new BinanceClient();
            _implementation.SetApiCredentials(
                Configuration.Instance.BinanceClientSettings.Credentials.Key,
                Configuration.Instance.BinanceClientSettings.Credentials.Secret);
        }

        // auto generated code
        #pragma warning disable
        public void SetApiCredentials(string apiKey, string apiSecret) => throw new NotImplementedException();

        public CallResult<long> Ping() => throw new NotImplementedException();

        public Task<CallResult<long>> PingAsync() => throw new NotImplementedException();

        public CallResult<DateTime> GetServerTime(bool resetAutoTimestamp = false) => throw new NotImplementedException();

        public Task<CallResult<DateTime>> GetServerTimeAsync(bool resetAutoTimestamp = false) => throw new NotImplementedException();

        public CallResult<BinanceExchangeInfo> GetExchangeInfo() => throw new NotImplementedException();

        public Task<CallResult<BinanceExchangeInfo>> GetExchangeInfoAsync() => throw new NotImplementedException();

        public CallResult<BinanceOrderBook> GetOrderBook(string symbol, int? limit = null) => throw new NotImplementedException();

        public Task<CallResult<BinanceOrderBook>> GetOrderBookAsync(string symbol, int? limit = null) => throw new NotImplementedException();

        public CallResult<BinanceAggregatedTrades[]> GetAggregatedTrades(string symbol, long? fromId = null, DateTime? startTime = null, DateTime? endTime = null,
            int? limit = null) => throw new NotImplementedException();

        public Task<CallResult<BinanceAggregatedTrades[]>> GetAggregatedTradesAsync(string symbol, long? fromId = null, DateTime? startTime = null, DateTime? endTime = null,
            int? limit = null) => throw new NotImplementedException();

        public CallResult<BinanceRecentTrade[]> GetRecentTrades(string symbol, int? limit = null) => throw new NotImplementedException();

        public Task<CallResult<BinanceRecentTrade[]>> GetRecentTradesAsync(string symbol, int? limit = null) => throw new NotImplementedException();

        public CallResult<BinanceRecentTrade[]> GetHistoricalTrades(string symbol, int? limit = null, long? fromId = null) => throw new NotImplementedException();

        public Task<CallResult<BinanceRecentTrade[]>> GetHistoricalTradesAsync(string symbol, int? limit = null, long? fromId = null) => throw new NotImplementedException();

        public CallResult<BinanceKline[]> GetKlines(string symbol, KlineInterval interval, DateTime? startTime = null, DateTime? endTime = null,
            int? limit = null) => throw new NotImplementedException();

        public Task<CallResult<BinanceKline[]>> GetKlinesAsync(string symbol, KlineInterval interval, DateTime? startTime = null, DateTime? endTime = null,
            int? limit = null) => throw new NotImplementedException();

        public CallResult<Binance24HPrice> Get24HPrice(string symbol) => throw new NotImplementedException();

        public Task<CallResult<Binance24HPrice>> Get24HPriceAsync(string symbol) => throw new NotImplementedException();

        public CallResult<Binance24HPrice[]> Get24HPricesList() => throw new NotImplementedException();

        public Task<CallResult<Binance24HPrice[]>> Get24HPricesListAsync() => throw new NotImplementedException();

        public CallResult<BinancePrice> GetPrice(string symbol) => throw new NotImplementedException();

        public Task<CallResult<BinancePrice>> GetPriceAsync(string symbol) => throw new NotImplementedException();

        public CallResult<BinancePrice[]> GetAllPrices() => throw new NotImplementedException();

        public Task<CallResult<BinancePrice[]>> GetAllPricesAsync() => throw new NotImplementedException();

        public CallResult<BinanceBookPrice> GetBookPrice(string symbol) => throw new NotImplementedException();

        public Task<CallResult<BinanceBookPrice>> GetBookPriceAsync(string symbol) => throw new NotImplementedException();

        public CallResult<BinanceBookPrice[]> GetAllBookPrices() => throw new NotImplementedException();

        public Task<CallResult<BinanceBookPrice[]>> GetAllBookPricesAsync() => throw new NotImplementedException();

        public CallResult<BinanceOrder[]> GetOpenOrders(string symbol = null, int? receiveWindow = null) => throw new NotImplementedException();

        public Task<CallResult<BinanceOrder[]>> GetOpenOrdersAsync(string symbol = null, int? receiveWindow = null) => throw new NotImplementedException();

        public CallResult<BinanceOrder[]> GetAllOrders(string symbol, long? orderId = null, int? limit = null, int? receiveWindow = null) => throw new NotImplementedException();

        public Task<CallResult<BinanceOrder[]>> GetAllOrdersAsync(string symbol, long? orderId = null, int? limit = null, int? receiveWindow = null) => throw new NotImplementedException();

        public CallResult<BinancePlacedOrder> PlaceOrder(string symbol, OrderSide side, OrderType type,
            decimal quantity, string newClientOrderId = null,
            decimal? price = null, TimeInForce? timeInForce = null, decimal? stopPrice = null,
            decimal? icebergQty = null,
            OrderResponseType? orderResponseType = null, int? receiveWindow = null)
        {
            var order = _implementation.PlaceTestOrder(symbol, side, type, quantity, newClientOrderId, price, timeInForce,
                stopPrice, icebergQty, orderResponseType, receiveWindow);

            if (order.Success)
            {
                var data = order.Data;
                data.OrderId = _orderIdGenerator.Next();

                // PlaceTestOrder does not propagate this data, set it manually.
                data.Type = type;
                data.Side = side;
                var ev = new OrderUpdate(
                    orderId: data.OrderId,
                    tradeId: 0,
                    orderStatus: data.Type == OrderType.Market ? OrderUpdate.OrderStatus.Filled : OrderUpdate.OrderStatus.New,
                    orderType: BinanceUtilities.ToInternal(data.Type),
                    createdTimeStamp: 0,
                    setPrice: data.Price,
                    side: BinanceUtilities.ToInternal(data.Side),
                    pair: TradingPair.Parse(symbol),
                    setQuantity: data.OriginalQuantity);
                _parent.ScheduleObserverEvent(ev);
            }

            return order;
        }

        public Task<CallResult<BinancePlacedOrder>> PlaceOrderAsync(string symbol, OrderSide side, OrderType type, decimal quantity, string newClientOrderId = null,
            decimal? price = null, TimeInForce? timeInForce = null, decimal? stopPrice = null, decimal? icebergQty = null,
            OrderResponseType? orderResponseType = null, int? receiveWindow = null) => throw new NotImplementedException();

        public CallResult<BinancePlacedOrder> PlaceTestOrder(string symbol, OrderSide side, OrderType type, decimal quantity,
            string newClientOrderId = null, decimal? price = null, TimeInForce? timeInForce = null, decimal? stopPrice = null,
            decimal? icebergQty = null, OrderResponseType? orderResponseType = null, int? receiveWindow = null) => throw new NotImplementedException();

        public Task<CallResult<BinancePlacedOrder>> PlaceTestOrderAsync(string symbol, OrderSide side, OrderType type, decimal quantity,
            string newClientOrderId = null, decimal? price = null, TimeInForce? timeInForce = null, decimal? stopPrice = null,
            decimal? icebergQty = null, OrderResponseType? orderResponseType = null, int? receiveWindow = null) => throw new NotImplementedException();

        public CallResult<BinanceOrder> QueryOrder(string symbol, long? orderId = null, string origClientOrderId = null, long? receiveWindow = null) => throw new NotImplementedException();

        public Task<CallResult<BinanceOrder>> QueryOrderAsync(string symbol, long? orderId = null, string origClientOrderId = null, long? receiveWindow = null) => throw new NotImplementedException();

        public CallResult<BinanceCanceledOrder> CancelOrder(string symbol, long? orderId = null, string origClientOrderId = null,
            string newClientOrderId = null, long? receiveWindow = null) => throw new NotImplementedException();

        public Task<CallResult<BinanceCanceledOrder>> CancelOrderAsync(string symbol, long? orderId = null, string origClientOrderId = null,
            string newClientOrderId = null, long? receiveWindow = null) => throw new NotImplementedException();

        public CallResult<BinanceAccountInfo> GetAccountInfo(long? receiveWindow = null) => throw new NotImplementedException();

        public Task<CallResult<BinanceAccountInfo>> GetAccountInfoAsync(long? receiveWindow = null) => throw new NotImplementedException();

        public CallResult<BinanceTrade[]> GetMyTrades(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null,
            long? fromId = null, long? receiveWindow = null) => throw new NotImplementedException();

        public Task<CallResult<BinanceTrade[]>> GetMyTradesAsync(string symbol, DateTime? startTime = null, DateTime? endTime = null, int? limit = null,
            long? fromId = null, long? receiveWindow = null) => throw new NotImplementedException();

        public CallResult<BinanceWithdrawalPlaced> Withdraw(string asset, string address, decimal amount, string addressTag = null, string name = null,
            int? receiveWindow = null) => throw new NotImplementedException();

        public Task<CallResult<BinanceWithdrawalPlaced>> WithdrawAsync(string asset, string address, decimal amount, string addressTag = null, string name = null,
            int? receiveWindow = null) => throw new NotImplementedException();

        public CallResult<BinanceDepositList> GetDepositHistory(string asset = null, DepositStatus? status = null, DateTime? startTime = null,
            DateTime? endTime = null, int? receiveWindow = null) => throw new NotImplementedException();

        public Task<CallResult<BinanceDepositList>> GetDepositHistoryAsync(string asset = null, DepositStatus? status = null, DateTime? startTime = null,
            DateTime? endTime = null, int? receiveWindow = null) => throw new NotImplementedException();

        public CallResult<BinanceWithdrawalList> GetWithdrawHistory(string asset = null, WithdrawalStatus? status = null, DateTime? startTime = null,
            DateTime? endTime = null, int? receiveWindow = null) => throw new NotImplementedException();

        public Task<CallResult<BinanceWithdrawalList>> GetWithdrawHistoryAsync(string asset = null, WithdrawalStatus? status = null, DateTime? startTime = null,
            DateTime? endTime = null, int? receiveWindow = null) => throw new NotImplementedException();

        public CallResult<BinanceDepositAddress> GetDepositAddress(string asset, int? receiveWindow = null) => throw new NotImplementedException();

        public Task<CallResult<BinanceDepositAddress>> GetDepositAddressAsync(string asset, int? receiveWindow = null) => throw new NotImplementedException();

        public CallResult<BinanceTradeFee[]> GetTradeFee(string asset = null, int? receiveWindow = null) => throw new NotImplementedException();

        public Task<CallResult<BinanceTradeFee[]>> GetTradeFeeAsync(string asset = null, int? receiveWindow = null) => throw new NotImplementedException();

        public CallResult<Dictionary<string, BinanceAssetDetails>> GetAssetDetails(int? receiveWindow = null) => throw new NotImplementedException();

        public Task<CallResult<Dictionary<string, BinanceAssetDetails>>> GetAssetDetailsAsync(int? receiveWindow = null) => throw new NotImplementedException();

        public CallResult<BinanceAccountStatus> GetAccountStatus(int? receiveWindow = null) => throw new NotImplementedException();

        public Task<CallResult<BinanceAccountStatus>> GetAccountStatusAsync(int? receiveWindow = null) => throw new NotImplementedException();

        public CallResult<BinanceSystemStatus> GetSystemStatus() => throw new NotImplementedException();

        public Task<CallResult<BinanceSystemStatus>> GetSystemStatusAsync() => throw new NotImplementedException();

        public CallResult<BinanceDustLog[]> GetDustLog(int? receiveWindow = null) => throw new NotImplementedException();

        public Task<CallResult<BinanceDustLog[]>> GetDustLogAsync(int? receiveWindow = null) => throw new NotImplementedException();

        public CallResult<string> StartUserStream() => throw new NotImplementedException();

        public Task<CallResult<string>> StartUserStreamAsync() => throw new NotImplementedException();

        public CallResult<object> KeepAliveUserStream(string listenKey) => throw new NotImplementedException();

        public Task<CallResult<object>> KeepAliveUserStreamAsync(string listenKey) => throw new NotImplementedException();

        public CallResult<object> StopUserStream(string listenKey) => throw new NotImplementedException();

        public Task<CallResult<object>> StopUserStreamAsync(string listenKey) => throw new NotImplementedException();

        public void AddRateLimiter(IRateLimiter limiter) => throw new NotImplementedException();

        public void RemoveRateLimiters() => throw new NotImplementedException();

        public void Dispose() => _implementation?.Dispose();

        public IRequestFactory RequestFactory { get; set; }
        #pragma warning restore
    }
}