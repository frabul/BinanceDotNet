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
        int MaxRequestsPerMinute = 0;

        public int MaxOrdersPerMinute { get; }

        public RateLimiter(int requestsLimit, int ordersLimit)
        {
            MaxRequestsPerMinute = requestsLimit;
            MaxOrdersPerMinute = ordersLimit;
            RequestsSemaphore = new SemaphoreSlim(requestsLimit, requestsLimit);
            OrdersSemaphore = new SemaphoreSlim(ordersLimit, ordersLimit);
        }
#pragma warning disable CS4014
        public async Task Requests(int weight)
        {
            await RqSem.WaitAsync();
            while (RequestsQueue.Count > MaxRequestsPerMinute + weight)
            {
                while (DateTime.Now - RequestsQueue.Peek() > TimeSpan.FromMinutes(1))
                    RequestsQueue.Dequeue();
                if (RequestsQueue.Count > MaxRequestsPerMinute + weight)
                    await Task.Delay(100);
            }
            for (int i = 0; i < weight; i++)
                RequestsQueue.Enqueue(DateTime.Now);
            RqSem.Release();




        }

        async Task ReleaseRequest(int weight)
        {
            await Task.Delay(60000);
            for (int i = 0; i < weight; i++)
            {
                RequestsSemaphore.Release();
            }
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
