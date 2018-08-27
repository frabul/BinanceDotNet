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
        SemaphoreSlim RequestsSemaphore;
        SemaphoreSlim OrdersSemaphore;
        Queue<DateTime> RequestsQueue = new Queue<DateTime>();
        int MaxRequestsPerInterval = 0;
        TimeSpan Interval = TimeSpan.Zero;
        public int MaxOrdersPerMinute { get; }

        public RateLimiter(int requestsLimit, int ordersLimit)
        {

            MaxOrdersPerMinute = ordersLimit;
            RequestsSemaphore = new SemaphoreSlim(requestsLimit, requestsLimit);
            OrdersSemaphore = new SemaphoreSlim(ordersLimit, ordersLimit);

            Interval = TimeSpan.FromMilliseconds(60000 / 10);
            MaxRequestsPerInterval = requestsLimit / 10;
        }
#pragma warning disable CS4014
        public async Task Requests(int weight)
        {
            await RqSem.WaitAsync();
            while (RequestsQueue.Count + weight > MaxRequestsPerInterval)
            {
                while (RequestsQueue.Count > 0 && DateTime.Now - RequestsQueue.Peek() > Interval)
                    RequestsQueue.Dequeue();
                if (RequestsQueue.Count + weight > MaxRequestsPerInterval)
                    await Task.Delay(200);
            }
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
                while (RequestsQueue.Count > 0 && DateTime.Now - RequestsQueue.Peek() > Interval)
                    RequestsQueue.Dequeue();
                int reqs = RequestsQueue.Count;
                RqSem.Release();
                return reqs * 10;
            }
            else
                return MaxRequestsPerInterval * 10;
        }
        public int GetOrdersRate()
        {
           return OrdersSemaphore.;
        }
        public async Task WaitOrder()
        {
            await OrdersSemaphore.WaitAsync();
            ReleaseOrder();
        }

        async Task ReleaseOrder()
        {
            await Task.Delay(1000);
            OrdersSemaphore.Release();
        }

    }
}
