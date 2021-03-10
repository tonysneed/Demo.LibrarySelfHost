# ASP.NET Core Self-Hosting with a Class Library and Worker Service

Demonstrates how to self-host an ASP.NET Core app using a .NET 5 Class Library and Worker Service.

### Class Library

1. Create a new .NET 5 Class Library project called **ServiceLibrary**.
   - Add `.Web` to  Microsoft.NET.Sdk
     - _You can also start with a Web API project and change it to a Class Library._
    ```xml
    <Project Sdk="Microsoft.NET.Sdk.Web">

      <PropertyGroup>
        <TargetFramework>net5.0</TargetFramework>
      </PropertyGroup>

    </Project>
    ```

2. Add a `Startup` class to configure web host.

    ```csharp
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
    ```

3. Add a `GreetingController` class to a `Controllers` folder.

    ```csharp
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
    ```

4. Add a `GreetingService` class.

    ```csharp
    public class GreetingService
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
    ```

### Worker Service

1. Add a _Worker Service_ project called **ServiceWorker**.
    - Reference the **ServiceLibrary** project.

2. Update `Program.CreateHostBuilder` to use `GreeterService`.

    ```csharp
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>

            // Use GreetingService to create host builder
            GreetingService.CreateHostBuilder(args)

                // Register services
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                });
    }
    ```

3. Run the **WorkerService** project (Ctrl+F5 in VS).
   - Browse to http://localhost:5000/greeting.
   - It should display the default `Hello World!` greeting.

### Console Client

1. Add a _Console_ project called **ServiceClient**.
    - Use `HttpClient` to create `Post` and `Get` requests.

    ```csharp
    class Program
    {
        static async Task Main(string[] args)
        {
            // Create http client
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5000/greeting/"),
            };
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            // Get name from the user
            string name = GetNameFromUser();

            while (name?.Length > 0)
            {
                // Set greeting
                var content = new StringContent(JsonSerializer.Serialize($"Hello {name}"), Encoding.UTF8, "application/json");
                var postResponse = await client.PostAsync("", content);
                postResponse.EnsureSuccessStatusCode();

                // Get greeting
                var getResponse = client.GetAsync("").Result;
                getResponse.EnsureSuccessStatusCode();
                var greeting = getResponse.Content.ReadAsStringAsync().Result;
                Console.WriteLine($"Greeting: {greeting}");

                // Get name again
                Console.WriteLine();
                name = GetNameFromUser();
            }
        }

        private static string GetNameFromUser()
        {
            Console.WriteLine("Enter a name:");
            return Console.ReadLine();
        }
    }
    ```

3. Run both the **ServiceClient** **WorkerService** projects (Ctrl+F5 in VS).
   - Enter names as prompted
   - It should display the corresponding greeting.
   - Browse to http://localhost:5000/greeting to view the greeting.
