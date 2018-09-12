using System;
using Binance.Net.Objects;

namespace SpreadShare.BinanceServices
{
    abstract class AbstractUserService : IUserService
    {   
        public EventHandler<BinanceStreamOrderUpdate> OrderUpdateHandler;
        public abstract void Start();

        protected void OnOrderUpdate(BinanceStreamOrderUpdate e) 
        {
             OrderUpdateHandler?.Invoke(this, e);
        }
    }
}