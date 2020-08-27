using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinanceExchange.API.Converter;
using BinanceExchange.API.Models.WebSocket.Interfaces;
using Newtonsoft.Json;

namespace BinanceExchange.API.Models.WebSocket
{
    public class OutboundAccountPosition : IWebSocketResponse
    {
        [DataMember(Order = 1)]
        [JsonProperty(PropertyName = "e")]
        public string EventType { get; set; }

        [DataMember(Order = 2)]
        [JsonProperty(PropertyName = "E")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime EventTime { get; set; }

        [DataMember(Order = 3)]
        [JsonProperty(PropertyName = "U")]
        [JsonConverter(typeof(EpochTimeConverter))]
        public DateTime LastAccountUpdate { get; set; }

        [DataMember(Order = 4)]
        [JsonProperty(PropertyName = "B")]
        public List<BalanceResponseData> Balances { get; set; } 
    }
}