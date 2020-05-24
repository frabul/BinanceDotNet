using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BinanceExchange.API.Client
{

    class RateLimiter
    {
        SemaphoreSlim RqSem = new SemaphoreSlim(1, 1); 
        SemaphoreSlim OrdersSemaphore; 
        Queue<DateTime> RequestsQueue = new Queue<DateTime>();
        Queue<DateTime> OrdersQueue = new Queue<DateTime>();
        int MaxRequestsPerInterval = 0;
        TimeSpan RequestsInterval = TimeSpan.Zero;
        public int MaxOrdersPerInterval { get; }
        public TimeSpan OrdersInterval { get; private set; }

        private const int SubDivision = 2;
         
        public RateLimiter(int requestPerMinute, int ordersPerSecond)
        {

            MaxOrdersPerInterval = ordersPerSecond;
            OrdersInterval = TimeSpan.FromSeconds(1.1); 
            OrdersSemaphore = new SemaphoreSlim(ordersPerSecond, ordersPerSecond);

            RequestsInterval = TimeSpan.FromMilliseconds(60000 / SubDivision);
            MaxRequestsPerInterval = requestPerMinute / SubDivision;
        }
 
        public async Task Requests(int weight)
        {
            try
            {
                await RqSem.WaitAsync();
                do
                {
                    while (RequestsQueue.Count > 0 && DateTime.Now - RequestsQueue.Peek() > RequestsInterval)
                        RequestsQueue.Dequeue();
                    if (RequestsQueue.Count + weight > MaxRequestsPerInterval)
                        await Task.Delay(50);
                } while (RequestsQueue.Count + weight > MaxRequestsPerInterval);
                for (int i = 0; i < weight; i++)
                    RequestsQueue.Enqueue(DateTime.Now);
            }
            finally
            {
                RqSem.Release();
            } 
        }

        /// <summary>
        /// Get the number of weighted requests per minute
        /// </summary>
        /// <returns></returns>
        public int GetRequestRate()
        {
            if (RqSem.Wait(100))
            {
                while (RequestsQueue.Count > 0 && DateTime.Now - RequestsQueue.Peek() > RequestsInterval)
                    RequestsQueue.Dequeue();
                int reqs = RequestsQueue.Count;
                RqSem.Release();
                return reqs * SubDivision;
            }
            else
                return MaxRequestsPerInterval * SubDivision;
        }
         
        public int GetOrdersRate()
        {
            return MaxOrdersPerInterval - OrdersSemaphore.CurrentCount;
        }
        public async Task WaitOrder()
        {
            try
            {
                await OrdersSemaphore.WaitAsync();  
                do
                {
                    //delete orders that have been in the queue long enough
                    while (OrdersQueue.Count > 0 && DateTime.Now - OrdersQueue.Peek() > OrdersInterval)
                        OrdersQueue.Dequeue();
                    //if we are not still in the condition to post new order wait some time
                    if (OrdersQueue.Count + 1 > MaxOrdersPerInterval)
                        await Task.Delay(50);
                    //repeat untile we can post new order
                } while (OrdersQueue.Count + 1 > MaxOrdersPerInterval);
                //
                OrdersQueue.Enqueue(DateTime.Now); 
            }
            finally
            {
                OrdersSemaphore.Release();
            } 
        } 
    }
}
