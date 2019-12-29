using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using BinanceExchange.API.Converter;
using BinanceExchange.API.Enums;
using BinanceExchange.API.Models.Response.Interfaces;
using Newtonsoft.Json.Converters;

namespace BinanceExchange.API.Models.Response
{ 
    public class ExchangeInfoSymbol
    {
        public string symbol { get; set; }
        public string status { get; set; }
        public string baseAsset { get; set; }
        public int baseAssetPrecision { get; set; }
        public string quoteAsset { get; set; }
        public int quotePrecision { get; set; }
        public int baseCommissionPrecision { get; set; }
        public int quoteCommissionPrecision { get; set; }
        public bool icebergAllowed { get; set; }
        public bool ocoAllowed { get; set; }
        public bool quoteOrderQtyMarketAllowed { get; set; }
        public bool isSpotTradingAllowed { get; set; }
        public bool isMarginTradingAllowed { get; set; }

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public List<ExchangeInfoOrderType> orderTypes { get; set; }

        [JsonProperty(ItemConverterType = typeof(ExchangeInfoSymbolFilterConverter))]
        public List<ExchangeInfoSymbolFilter> filters { get; set; }
    }
}
