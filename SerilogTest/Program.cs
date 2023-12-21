using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.Http;

namespace SerilogTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            ILogger Log = new LoggerConfiguration()
                            .MinimumLevel.Information()
                            .WriteTo.Http(
                                requestUri: "http://localhost:5001/api/Process/",
                                queueLimitBytes: null,
                                httpClient: new CustomHttpClient(),
                                logEventLimitBytes: 512000,
                                period: TimeSpan.FromMinutes(5)
                            )
                            .CreateLogger()
                            .ForContext<Program>();

            Console.WriteLine("Start test");
            while (true)
            {
                Console.WriteLine("enter key");
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Q) break;
                Log.Information("App start");
                Log.Warning("App warn");
                Log.Error("App error");
                Log.Fatal("App fatal");
                Console.WriteLine("Write to log");
            }
            Console.WriteLine("end");

            Console.ReadKey();
        }
    }

    public class CustomHttpClient : IHttpClient
    {
        private readonly HttpClient httpClient;

        public CustomHttpClient() => httpClient = new HttpClient();

        public void Configure(IConfiguration configuration) =>
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "Test");

        public async Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream)
        {
            var content = new StreamContent(contentStream);
            content.Headers.Add("Content-Type", "application/json");

            var response = await httpClient
                .PostAsync(requestUri, content)
                .ConfigureAwait(false);

            return response;
        }

        public void Dispose() => httpClient?.Dispose();
    }
}
