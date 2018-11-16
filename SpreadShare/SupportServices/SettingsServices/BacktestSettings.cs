using SpreadShare.Models.Trading;

namespace SpreadShare.SupportServices.SettingsServices
{
    internal class BacktestSettings
    {
        public Portfolio InitialPortfolio { get; set; }
        public long BeginTimeStamp { get; set; }
        
        public long EndTimeStamp { get; set; }
    }
}