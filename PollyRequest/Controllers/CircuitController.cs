using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Bulkhead;
using Polly.Caching;
using PollyRequest.Policies;

namespace PollyRequest
{
    [Route("api/[controller]")]
    [ApiController]
    public class CircuitBreakerController : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                HttpClient client = new HttpClient();

                HttpResponseMessage response = await ClientPolicy.SimpleCircuitBreakerPolicy.ExecuteAsync(() => client.GetAsync("http://localhost:5089/Hello"));
                Console.WriteLine((response.IsSuccessStatusCode ? "Received Success Greetings " : "Received Error ") + DateTime.Now);
                return Ok(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ErrorCircuitBreakerController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                HttpClient client = new HttpClient();

                HttpResponseMessage response = await ClientPolicy.ExceptionBreaker.ExecuteAsync(() => client.GetAsync("http://localhost:5089/Hello"));
                Console.WriteLine((response.IsSuccessStatusCode ? "Received Success Greetings " : "Received Error ") + DateTime.Now);
                return Ok(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
    [Route("api/[controller]")]
    [ApiController]
    public class WrapperCircuitBreakerController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await ClientPolicy.SimpleCircuitBreakerPolicy.
                    WrapAsync(ClientPolicy.ExceptionBreaker)
                    .ExecuteAsync(() => client.GetAsync("http://localhost:5089/Hello"));
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Received Success Greetings " + DateTime.Now);
                    string res = await response.Content.ReadAsStringAsync();
                    return Ok(res);
                }
                else
                {
                    Console.WriteLine("Received Error " + DateTime.Now);
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
    [Route("api/[controller]")]
    [ApiController]
    public class WrapperCircuitBreakerEqualTimeDelayController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await ClientPolicy.EqualTimeDelayRetryPolicy.
                    WrapAsync(ClientPolicy.SimpleCircuitBreakerPolicy).
                    WrapAsync(ClientPolicy.ExceptionBreaker).
                    ExecuteAsync(() => client.GetAsync("http://localhost:5089/Hello"));
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Received Success Greetings " + DateTime.Now);
                    string res = await response.Content.ReadAsStringAsync();
                    return Ok(res);
                }
                else
                {
                    Console.WriteLine("Received Error " + DateTime.Now);
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class WrapperTimeOutController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                HttpClient client = new HttpClient();

                HttpResponseMessage response =
                            await
                                ClientPolicy.httpRetryPolicy.ExecuteAsync(() =>
                                    ClientPolicy.timeoutPolicy.ExecuteAsync(async token => await client.GetAsync("http://localhost:5089/Hello", token), CancellationToken.None));

                //HttpResponseMessage response = await ClientPolicy.ExpTimeDelayRetryPolicyHigh.WrapAsync(ClientPolicy.timeoutPolicy).ExecuteAsync(() => client.GetAsync("http://localhost:5089/Hello"));

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Received Success Greetings " + DateTime.Now);
                    string res = await response.Content.ReadAsStringAsync();
                    return Ok(res);
                }
                else
                {
                    Console.WriteLine("Received Error " + DateTime.Now);
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
    [Route("api/[controller]")]
    [ApiController]
    public class WrapperFallBackController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                HttpClient client = new HttpClient();

                HttpResponseMessage response = await ClientPolicy.NoRetryPolicy.WrapAsync(ClientPolicy.fallbackPolicy).ExecuteAsync(() => client.GetAsync("http://localhost:5089/Hello"));

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Received Success Greetings " + DateTime.Now);
                    string res = await response.Content.ReadAsStringAsync();
                    return Ok(res);
                }
                else
                {
                    Console.WriteLine("Received Error " + DateTime.Now);
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class MemoryCacheController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                //https://www.pluralsight.com/blog/software-development/intro-to-polly
                //Example of caching a result in local memory for 10 seconds

                //var memoryCache = new MemoryCache(new MemoryCacheOptions());
                //var memoryCacheProvider = new MemoryCacheProvider(memoryCache);

                //CachePolicy<int> cachePolicy =
                //    Policy.Cache<int>(memoryCacheProvider, TimeSpan.FromSeconds(10));

                //var result =
                //    cachePolicy.Execute(context =>
                //        QueryRemoteService(id), new Context($"QRS-{id}"));

                return Ok(StatusCodes.Status200OK);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class BulkHeadController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                //https://www.pluralsight.com/blog/software-development/intro-to-polly
                //Bulkhead Isolation policy with three execution slots and six queue slots
                BulkheadPolicy bulkheadPolicy = Policy.Bulkhead(3, 6);

                var result = bulkheadPolicy.Execute(() => ResourceHeavyRequest());

                return Ok(StatusCodes.Status200OK);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ActionResult> ResourceHeavyRequest()
        {
            return Ok(StatusCodes.Status200OK);
        }
    }
}
