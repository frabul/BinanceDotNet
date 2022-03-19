using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace BinanceExchange.API.Models.WebSocket
{
 
    public class BinanceCombinedWebsocketData
    {
        [JsonPropertyName("stream")]
        [JsonProperty("stream")]
        public string StreamName { get; set; } 

        [JsonProperty("data")]
        [JsonPropertyName("data")]
        public System.Text.Json.JsonDocument RawData { get; set; }
    }
}
