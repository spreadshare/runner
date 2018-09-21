using System;
using System.Threading.Tasks;
using Binance.Net.Objects;
using SpreadShare.Models;

namespace SpreadShare.BinanceServices
{
    internal abstract class AbstractUserService : IUserService
    {   
        public EventHandler<BinanceStreamOrderUpdate> OrderUpdateHandler;

        public abstract Task Start();

        public abstract Assets GetPortfolio();

        protected void OnOrderUpdate(BinanceStreamOrderUpdate e) 
        {
             OrderUpdateHandler?.Invoke(this, e);
        }
    }
}