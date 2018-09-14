using System;
using Binance.Net.Objects;
using SpreadShare.Models;

namespace SpreadShare.BinanceServices
{
    abstract class AbstractUserService : IUserService
    {   
        public EventHandler<BinanceStreamOrderUpdate> OrderUpdateHandler;

        public abstract void Start();

        public abstract Assets GetPortfolio();

        protected void OnOrderUpdate(BinanceStreamOrderUpdate e) 
        {
             OrderUpdateHandler?.Invoke(this, e);
        }
    }
}