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
        int MaxRequestsPerInterval = 0;
        TimeSpan RequestsInterval = TimeSpan.Zero;
        public int MaxOrdersPerMinute { get; }
        private const int SubDivision = 2;



        public RateLimiter(int requestPerMinute, int ordersPerSecond)
        {

            MaxOrdersPerMinute = ordersPerSecond;
        
            OrdersSemaphore = new SemaphoreSlim(ordersPerSecond, ordersPerSecond);

            RequestsInterval = TimeSpan.FromMilliseconds(60000 / SubDivision);
            MaxRequestsPerInterval = requestPerMinute / SubDivision;
        }
#pragma warning disable CS4014
        public async Task Requests(int weight)
        {
            await RqSem.WaitAsync();
            do
            {
                while (RequestsQueue.Count > 0 && DateTime.Now - RequestsQueue.Peek() > RequestsInterval)
                    RequestsQueue.Dequeue();
                if (RequestsQueue.Count + weight > MaxRequestsPerInterval)
                    await Task.Delay(200);
            } while (RequestsQueue.Count + weight > MaxRequestsPerInterval);
            for (int i = 0; i < weight; i++)
                RequestsQueue.Enqueue(DateTime.Now);
            RqSem.Release();
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
            return MaxOrdersPerMinute - OrdersSemaphore.CurrentCount;
        }
        public async Task WaitOrder()
        {
            await OrdersSemaphore.WaitAsync();
            ReleaseOrder();
        }

        async Task ReleaseOrder()
        {
            await Task.Delay(1500);
            OrdersSemaphore.Release();
        }

    }
}
