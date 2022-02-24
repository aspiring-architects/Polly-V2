using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Caching;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;

namespace PollyRequest.Policies
{
    public static class ClientPolicy
    {
        public static AsyncRetryPolicy<HttpResponseMessage> NoRetryPolicy { get; }
        public static AsyncRetryPolicy<HttpResponseMessage> ImmediateRetryPolicy { get; }
        public static AsyncRetryPolicy<HttpResponseMessage> EqualTimeDelayRetryPolicy { get; }
        public static AsyncRetryPolicy<HttpResponseMessage> ExpTimeDelayRetryPolicy { get; }
        public static AsyncRetryPolicy<HttpResponseMessage> ExpTimeDelayRetryPolicyHigh { get; }
        public static AsyncCircuitBreakerPolicy<HttpResponseMessage> SimpleCircuitBreakerPolicy { get; }

        public static AsyncCircuitBreakerPolicy<HttpResponseMessage> AdvancedCircuitBreakerPolicy { get; }

        public static AsyncCircuitBreakerPolicy ExceptionBreaker { get; }

        public static AsyncFallbackPolicy fallback { get; }

        public static AsyncTimeoutPolicy timeoutPolicy { get; }

        public static AsyncRetryPolicy<HttpResponseMessage> httpRetryPolicy;

        public static IAsyncPolicy<HttpResponseMessage> fallbackPolicy;
        static ClientPolicy()
        {
            NoRetryPolicy = Policy.HandleResult<HttpResponseMessage>(res => !res.IsSuccessStatusCode)
               .RetryAsync(0);

            ImmediateRetryPolicy = Policy.HandleResult<HttpResponseMessage>(res => !res.IsSuccessStatusCode)
                .RetryAsync(2);

            EqualTimeDelayRetryPolicy = Policy.HandleResult<HttpResponseMessage>(res => !res.IsSuccessStatusCode)
                .WaitAndRetryAsync(2, delay => TimeSpan.FromSeconds(5));

            ExpTimeDelayRetryPolicy = Policy.HandleResult<HttpResponseMessage>(res => !res.IsSuccessStatusCode)
                .WaitAndRetryAsync(2, delay => TimeSpan.FromSeconds(Math.Pow(2, delay)));

            ExpTimeDelayRetryPolicyHigh = Policy.HandleResult<HttpResponseMessage>(res => !res.IsSuccessStatusCode)
                .WaitAndRetryAsync(2, delay => TimeSpan.FromSeconds(Math.Pow(5, delay)));

            /*
             * Polly offers two variations of the policy: the basic circuit breaker that cuts the connection 
             * if a specified number of consecutive failures occur, and the advanced circuit breaker that cuts 
             * the connection when a specified percentage of errors occur over a specified period 
             * and when a minimum number of requests have occurred in that period.
             */

            //if 2 consecutive errors occur, the circuit is cut for 30 seconds
            SimpleCircuitBreakerPolicy = Policy.HandleResult<HttpResponseMessage>(res => !res.IsSuccessStatusCode).CircuitBreakerAsync(4, TimeSpan.FromSeconds(20),
                    onBreak: (ex, @break) =>
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(DateTime.Now.ToString() + " " + $"{"Break",-10}{@break,-10:ss}: {ex.GetType().Name}");
                        Console.ResetColor();
                    },
                    onReset: () =>
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{"Reset",-10}" + " " + DateTime.Now.ToString());
                        Console.ResetColor();
                    },
                    onHalfOpen: () =>
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"{"HalfOpen",-10}" + " " + DateTime.Now.ToString());
                        Console.ResetColor();
                    });
            ExceptionBreaker = Policy.Handle<Exception>().CircuitBreakerAsync(2, TimeSpan.FromSeconds(30),
                onBreak: (ex, @break) =>
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(DateTime.Now.ToString() + " " + $"{"Break",-10}{@break,-10:ss}: {ex.GetType().Name}");
                    Console.ResetColor();
                },
                onReset: () =>
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{"Reset",-10}" + " " + DateTime.Now.ToString());
                    Console.ResetColor();
                },
                onHalfOpen: () =>
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{"HalfOpen",-10}" + " " + DateTime.Now.ToString());
                    Console.ResetColor();
                });

            fallbackPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
          .FallbackAsync(FallbackAction, OnFallbackAsync);


            httpRetryPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .Or<TimeoutRejectedException>()
                    .RetryAsync(0);

            timeoutPolicy = Policy.TimeoutAsync(3);


            //The circuit will be cut if 30% of requests fail in a 60 second window,
            //with a minimum of 9 requests in the 60 second window, then the circuit should be cut for 30 seconds
            AdvancedCircuitBreakerPolicy = Policy.HandleResult<HttpResponseMessage>(res => !res.IsSuccessStatusCode)
                .AdvancedCircuitBreakerAsync(0.30, TimeSpan.FromSeconds(60), 9, TimeSpan.FromSeconds(30));
        }

        private static Task OnFallbackAsync(DelegateResult<HttpResponseMessage> response, Context context)
        {
            Console.WriteLine("Calling fallback action...");
            return Task.CompletedTask;
        }

        private static Task<HttpResponseMessage> FallbackAction(DelegateResult<HttpResponseMessage> responseToFailedRequest, Context context, CancellationToken cancellationToken)
        {
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage(responseToFailedRequest.Result.StatusCode)
            {
                Content = new StringContent($"The fallback executed, the original error was {responseToFailedRequest.Result.ReasonPhrase}")
            };
            Console.WriteLine(httpResponseMessage.Content.ReadAsStringAsync().Result);
            return Task.FromResult(httpResponseMessage);
        }
    }
}
