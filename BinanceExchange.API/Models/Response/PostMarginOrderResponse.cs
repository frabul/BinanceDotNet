using BinanceExchange.API.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BinanceExchange.API.Models.Response
{
    public class PostMarginOrderResponse_Ack
    {
        public string Symbol { get; set; }
        public long OrderId { get; set; }
        public string ClientOrderdId { get; set; }
        public bool IsIsolated { get; set; }
        public long TransactionTime { get; set; }
    }
    public class PostMarginOrderResponse_Result : PostMarginOrderResponse_Ack
    {
        public decimal Price { get; set; }
        public decimal OrigQty { get; set; }
        public decimal ExecutedQty { get; set; }
        public decimal CummulativeQuoteQty { get; set; }
        public OrderStatus Status { get; set; }
        public TimeInForce TimeInForce { get; set; }
        public OrderType Type { get; set; }
        public OrderSide Side { get; set; }
    }
}
