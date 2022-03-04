using System;
using System.Runtime.Serialization;
using BinanceExchange.API.Converter;
using Newtonsoft.Json;

namespace BinanceExchange.API.Models.Response
{
    /// <summary>
    /// Response providing Account Trade information
    /// </summary>
    [DataContract]
    public class AccountTradeReponse
    {
        public string Symbol { get; set; }
        public long Id { get; set; }

        public long OrderId { get; set; }

        public long OrderListId { get; set; }

        public decimal Price { get; set; }

        [JsonProperty(PropertyName = "qty")]
        public decimal Quantity { get; set; }

        [JsonProperty(PropertyName = "quoteQty")]
        public decimal QuoteQuantity { get; set; }

        public decimal Commission { get; set; }

        public string CommissionAsset { get; set; }

        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime Time { get; set; }

        public bool IsBuyer { get; set; }

        public bool IsMaker { get; set; }

        public bool IsBestMatch { get; set; }
    }
}
