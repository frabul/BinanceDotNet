using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BinanceExchange.API.Client
{

    class RateLimiter
    {
        SemaphoreSlim RequestsSemaphore;
        SemaphoreSlim OrdersSemaphore;
        public RateLimiter(int requestsLimit, int ordersLimit)
        {
            RequestsSemaphore = new SemaphoreSlim(requestsLimit, requestsLimit);
            OrdersSemaphore = new SemaphoreSlim(ordersLimit, ordersLimit);
        }
#pragma warning disable CS4014
        public async Task Requests(int weight)
        {
            for (int i = 0; i < weight; i++)
            {
                await RequestsSemaphore.WaitAsync();
            }
            ReleaseRequest(weight);
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
