using System;
using System.Runtime.Serialization;
using BinanceExchange.API.Converter;
using BinanceExchange.API.Enums;
using BinanceExchange.API.Models.Response.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BinanceExchange.API.Models.Response
{
    /// <summary>
    /// Response object received when querying a Binance order
    /// </summary>
    [DataContract]
    public class OrderResponse : IResponse
    { 
        public OrderResponse()
        {

        }
        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; }

        [JsonProperty(PropertyName = "orderId")]
        public long OrderId { get; set; }

        [JsonProperty(PropertyName = "clientOrderId")]
        public string ClientOrderId { get; set; }

        [JsonProperty(PropertyName = "price")]
        public decimal Price { get; set; }
     
        [JsonProperty(PropertyName = "origQty")]
        public decimal OriginalQuantity { get; set; }
         
        [JsonProperty(PropertyName = "executedQty")]
        public decimal ExecutedQuantity { get; set; }

        [JsonProperty(PropertyName = "status")]
        [JsonConverter(typeof(StringEnumConverter))] 
        public OrderStatus Status { get; set; }

        [JsonProperty(PropertyName = "timeInForce")]
        public TimeInForce TimeInForce { get; set; }

        [JsonProperty(PropertyName = "type")]
        [JsonConverter(typeof(StringEnumConverter))] 
        public OrderType Type { get; set; }

        [JsonProperty(PropertyName = "side")]
        public OrderSide Side { get; set; }

        [JsonProperty(PropertyName = "stopPrice")]
        public decimal StopPrice { get; set; }
         
        [JsonProperty(PropertyName = "icebergQty")]
        public decimal IcebergQuantity { get; set; }

        [JsonProperty(PropertyName = "time")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime Time { get; set; }

        [JsonProperty(PropertyName = "isWorking")]
        public bool IsWorking { get; set; }
    }
}