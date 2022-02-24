using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Polly;
using PollyRequest.Policies;

namespace PollyRequest.Properties
{

    [Route("[controller]")]
    [ApiController]
    public class AllRetryPolicyController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            ////Approach 1: Default
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("----------#1 : Default API Call-------");
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync("http://localhost:5089/Hello");
            Console.WriteLine((response.IsSuccessStatusCode ? "Received Success Greetings " : "Received Error ") + DateTime.Now);
            Console.WriteLine("-----------------------------------------------------\n\n");
            Console.ReadKey();

            //Approach 2: Immedicate Retry
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("----------#2 : Immediate Retry Policy API Call (3 Times)-------");
            response = await ClientPolicy.ImmediateRetryPolicy.ExecuteAsync(() => client.GetAsync("http://localhost:5089/Hello"));
            Console.WriteLine((response.IsSuccessStatusCode ? "Received Success Greetings " : "Received Error ") + DateTime.Now);
            Console.WriteLine("-----------------------------------------------------\n\n");
            Console.ReadKey();

            //Approach 3: Equal Time Delay Retry Policy
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("----------#3 : Equal Time Delay Retry Policy API Call (3 Times)-------");
            response = await ClientPolicy.EqualTimeDelayRetryPolicy.ExecuteAsync(() => client.GetAsync("http://localhost:5089/Hello"));
            Console.WriteLine((response.IsSuccessStatusCode ? "Received Success Greetings " : "Received Error ") + DateTime.Now);
            Console.WriteLine("-----------------------------------------------------\n\n");
            Console.ReadKey();

            //Approach 4: Exp Time Delay Retry Policy
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("----------#4 : Exponential Time Delay Retry Policy API Call (3 Times)-------");
            response = await ClientPolicy.ExpTimeDelayRetryPolicy.ExecuteAsync(() => client.GetAsync("http://localhost:5089/Hello"));
            Console.WriteLine((response.IsSuccessStatusCode ? "Received Success Greetings " : "Received Error ") + DateTime.Now);
            Console.WriteLine("-----------------------------------------------------\n\n");

            return Ok(await response.Content.ReadAsStringAsync());

        }       
    }    
}
