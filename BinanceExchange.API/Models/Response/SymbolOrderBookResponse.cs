using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace BinanceExchange.API.Models.Response
{

    public class SymbolOrderBookResponse
    { 
        public string Symbol { get; set; }
         
        public decimal BidPrice { get; set; }

        [JsonPropertyName("bidQty")]
        [JsonProperty(PropertyName = "bidQty")]
        public decimal BidQuantity { get; set; }

        public decimal AskPrice { get; set; }

        [JsonPropertyName("askQty")]
        [JsonProperty(PropertyName = "askQty")]
        public decimal AskQuantity { get; set; }
    }
}