using BinanceExchange.API.Converter;
using BinanceExchange.API.Enums;
using BinanceExchange.API.Models.Request.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BinanceExchange.API.Models.Request
{

    public enum SideEffectType
    {
        [EnumMember(Value = "NO_SIDE_EFFECT")]
        NO_SIDE_EFFECT,
        [EnumMember(Value = "MARGIN_BUY")]
        MARGIN_BUY,
        [EnumMember(Value = "AUTO_REPAY")]
        AUTO_REPAY,
    }
    public class PostMarginOrderRequest : IRequest
    {
        public string symbol { get; set; }

        public bool? isIsolated { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public OrderSide side { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public OrderType type { get; set; }

        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? quantity { get; set; }

        [JsonConverter(typeof(StringDecimalConverter))] 
        public decimal? price { get; set; }

        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? stopPrice { get; set; }

        public string newClientOrderId { get; set; }

        [JsonConverter(typeof(StringDecimalConverter))]
        public decimal? icebergQty { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public NewOrderResponseType? newOrderRespType { get; set; } = NewOrderResponseType.Acknowledge;

        [JsonConverter(typeof(StringEnumConverter))]
        public SideEffectType? sideEffectType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TimeInForce? timeInForce { get; set; }
        public long? recvWindow { get; set; } = 5000;  
    }

}
