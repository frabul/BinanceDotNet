using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
namespace BinanceExchange.API.Models.Response
{
    [DataContract]
    public class ExchangeInfoSymbolFilterMaxNumAlgoOrders : ExchangeInfoSymbolFilter
    {
        [JsonProperty("maxNumAlgoOrders")]
        public int MaxNumAlgoOrders { get; set; }
    }
}
