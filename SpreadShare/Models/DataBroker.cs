using System;

namespace SpreadShare.Models
{
    public class DataBroker {
        public DataBroker() {

        }

        /// <summary>
        /// Should process the candle in an internal datastructure
        /// </summary>
        public void OnCandle() {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="minutes"></param>
        public void GetPriceMovement(string symbol, long minutes) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="minutes"></param>
        public void GetVolumeTotal(string symbol, long minutes) {

        }
    }
}