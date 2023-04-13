namespace BinanceExchange.API.Models.Response
{
    public class ExchangeInfoSymbolFilterNotional : ExchangeInfoSymbolFilter
    {
        public decimal MinNotional { get; set; }
        public decimal MaxNotional { get; set; }
        public bool ApplyMinToMarket { get; set; }
        public bool ApplyMaxToMarket { get; set; }
        public decimal AvgPriceMins { get; set; }
    }
}