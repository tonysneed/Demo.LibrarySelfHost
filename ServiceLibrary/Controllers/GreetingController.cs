using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LibrarySelfHost.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GreetingController : ControllerBase
    {
        private readonly ILogger<GreetingController> _logger;

        // Greeting store
        private static string Greeting { get; set; } = "Hello World!";

        public GreetingController(ILogger<GreetingController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return new JsonResult(Greeting);
        }

        [HttpPost]
        public IActionResult Post([FromBody] string greeting)
        {
            _logger.LogInformation($"ServiceLibrary: Greeting set to {greeting}");

            Greeting = greeting;
            return new NoContentResult();
        }
    }
}
