using Microsoft.AspNetCore.Mvc;

namespace PollyResponse.Controllers
{
    [ApiController]
    [Route("[Controller]")]
    public class HelloController : Controller
    {
        [HttpGet]
        public IActionResult Greetings()
        {
            Random rnd = new Random();
            var x = rnd.NextInt64(1, 10);


            Thread.Sleep(5000);
            //if (x % 3 == 1)
            if(System.DateTime.Now.Minute % 2 == 0)
            {
                Console.WriteLine("Success " + DateTime.Now);
                return Ok("Hello Karl");
            }
            else
            {
                Console.WriteLine("Failure " + DateTime.Now); 
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
