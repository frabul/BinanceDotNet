using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceExchange.API.Models.Response
{ 
    //{
    //    "assetFullName": "USD coin",
    //     "assetName": "USDC",
    //     "isBorrowable": true,
    //     "isMortgageable": true,
    //     "userMinBorrow": "0.00000000",
    //     "userMinRepay": "0.00000000"
    //}, 
    public class CrossMarginAssetInfo
    {
        public string assetFullName { get; set; }
        public string AssetName { get; set; }
        public bool IsBorrowable { get; set; }
        public bool IsMortgageable { get; set; } 
        public decimal UserMinBorrow { get; set; }
        public decimal UserMinRepay { get; set; }

    }
    //{
    //    "base": "BNB",
    //    "id": 351637150141315861,
    //    "isBuyAllowed": true,
    //    "isMarginTrade": true,
    //    "isSellAllowed": true,
    //    "quote": "BTC",
    //    "symbol": "BNBBTC"
    //},
    public class CrossMarginPair
    {
        public string Base { get; set; }
        public long Id { get; set; }
        public bool isBuyAllowed { get; set; }
        public bool isMarginTrade { get; set; }
        public bool isSellAllowed { get; set; }
        public string quote { get; set; }
        public string symbol { get; set; }
    }

    public class MarginEndpoints
    {
        public const string GetAllCrossMarginAssets = "/sapi/v1/margin/allAssets";
        public const string GetAllCrossMarginPairs = "/sapi/v1/margin/allPairs";
    }
}
