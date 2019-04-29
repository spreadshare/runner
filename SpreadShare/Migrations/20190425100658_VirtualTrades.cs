using Microsoft.EntityFrameworkCore.Migrations;

namespace SpreadShare.Migrations
{
    /// <summary>
    /// Custom SQL migration that creates the 'Trade' view.
    /// </summary>
    public partial class VirtualTrades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"
                CREATE VIEW ""Trades"" AS
                   SELECT buys.""Pair"", sells.volumeqt / NULLIF(buys.volumeqt, 0) as ""Performance"", 
                           buys.volume as ""BuyVolume"", buys.volumeqt as ""BuyVolumeQuote"", 
                           sells.volume as ""SellVolume"", sells.volumeqt as ""SellVolumeQuote"", 
                           buys.""TradeId"", buys.""SessionId"", sells.end - buys.start as ""Duration"", 
                           COALESCE(buys.volumeqt / NULLIF(buys.volume,0),0) as ""AverageBuyPrice"",
                           COALESCE(sells.volumeqt / NULLIF(sells.volume,0),0) as ""AverageSellPrice"",
                           buys.commission as ""BuyCommission"",
                           buys.commission_asset as ""BuyCommissionAsset"",
                           sells.commission as ""SellCommission"",
                           sells.commission_asset as ""SellCommissionAsset"",
                           COALESCE(buys.count,0) + COALESCE(sells.count, 0) as ""OrderCount""
                    FROM 
                        (SELECT SUM(""LastFillPrice"" * ""LastFillIncrement"") as volumeqt, 
                                SUM(""LastFillIncrement"") as volume, ""Side"", ""TradeId"", ""SessionId"",
                                MAX(""FilledTimestamp"") as end, ""Pair"",
                                SUM(""Commission"") as commission,
                                (array_agg(DISTINCT ""CommissionAsset""))[1] as commission_asset,
                                COUNT(DISTINCT ""OrderId"") as count
                         FROM ""OrderEvents"" 
                         WHERE ""Side"" = 'Sell'
                         GROUP BY ""SessionId"", ""TradeId"", ""Side"", ""Pair"")
                            sells 
                    FULL OUTER JOIN 
                        (SELECT SUM(""LastFillPrice"" * ""LastFillIncrement"") as volumeqt,
                                SUM(""LastFillIncrement"") as volume, ""Side"", ""TradeId"", ""SessionId"",
                                MIN(""FilledTimestamp"") as start, ""Pair"",
                                SUM(""Commission"") as commission,
                                (array_agg(DISTINCT ""CommissionAsset""))[1] as commission_asset,
                                COUNT(DISTINCT ""OrderId"") as count
                        FROM ""OrderEvents"" 
                        WHERE ""Side"" = 'Buy'
                        GROUP BY ""SessionId"", ""TradeId"", ""Side"", ""Pair"")
                            buys 
                    ON      buys.""SessionId"" = sells.""SessionId"" 
                        AND buys.""TradeId"" = sells.""TradeId""; 
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW ""Trades""");
        }
    }
}
