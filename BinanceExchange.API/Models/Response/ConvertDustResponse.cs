using BinanceExchange.API.Models.Request.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceExchange.API.Models.Response
{
    //{
    //    "totalServiceCharge":"0.02102542",
    //    "totalTransfered":"1.05127099",
    //    "transferResult":[
    //        {
    //            "amount":"0.03000000",
    //            "fromAsset":"ETH",
    //            "operateTime":1563368549307,
    //            "serviceChargeAmount":"0.00500000",
    //            "tranId":2970932918,
    //            "transferedAmount":"0.25000000"
    //        },
    //        {
    //    "amount":"0.09000000",
    //            "fromAsset":"LTC",
    //            "operateTime":1563368549404,
    //            "serviceChargeAmount":"0.01548000",
    //            "tranId":2970932918,
    //            "transferedAmount":"0.77400000"
    //        },
    //        {
    //    "amount":"248.61878453",
    //            "fromAsset":"TRX",
    //            "operateTime":1563368549489,
    //            "serviceChargeAmount":"0.00054542",
    //            "tranId":2970932918,
    //            "transferedAmount":"0.02727099"
    //        }
    //    ]
    //}
    public class ConvertDustResponse : IRequest
    {
        public class TransferResult
        {
            public decimal amount { get; set; }
            public string fromAsset { get; set; }
            public long operateTime { get; set; }
            public decimal serviceChargeAmount { get; set; }
            public long tranId { get; set; }
            public decimal transferedAmount { get; set; }
        }

        public decimal totalServiceCharge { get; set; }
        public decimal totalTransfered { get; set; }
        public TransferResult[] transferResult { get; set; }

    }
}
