using System.Runtime.Serialization;
namespace BinanceExchange.API.Models.Response
{
    [DataContract]
    public class ExchangeInfoSymbolFilterNotional : ExchangeInfoSymbolFilter
    {
        [DataMember]
        public decimal MinNotional { get; set; } = -1;
        [DataMember]
        public decimal MaxNotional { get; set; }
        [DataMember]
        public bool ApplyMinToMarket { get; set; }
        [DataMember]
        public bool ApplyMaxToMarket { get; set; }
        [DataMember]
        public decimal AvgPriceMins { get; set; }
    }
}