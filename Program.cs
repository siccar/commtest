using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Siccar.Application.Client.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Siccar.Common.ServiceClients;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Siccar.Common.Exceptions;

namespace CommTest.basic
{
    class Program
    {
        static IConfiguration _configuration;
        static string bearer = "";

        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("Siccar System Checker v1.0");

            _configuration = SetupConfiguration();

            var serilog = new LoggerConfiguration()
             .MinimumLevel.Error()
             .Enrich.FromLogContext()
             .WriteTo.File("output.log", outputTemplate: "{SourceContext}{NewLine}{Message}{NewLine}")
             .CreateLogger();

            // Lets go raw to start with...
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError() // HttpRequestException, 5XX and 408
                .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(retryAttempt));

            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(configure => _configuration.GetSection("Logging"));
                    services.AddHttpClient("siccarClient", s =>
                       s.BaseAddress = new Uri(_configuration["SiccarService"]))
                        .AddPolicyHandler(retryPolicy);
                    services.AddSiccarSDKStateManagement(_configuration); // gets added last
                })
                .UseConsoleLifetime();

            var _host = builder.Build();

            var _serviceProvider = _host.Services.CreateScope().ServiceProvider;

            Console.WriteLine("Examining configuration and connectivity");
            var authn = new AuthN(_configuration);

            var baseClient = _serviceProvider.GetService<SiccarBaseClient>();

            var tenantTests = new TenantTests(_serviceProvider);
            if (!tenantTests.CanConnect(_configuration["SiccarService"]))
            {
                Console.WriteLine($"Test client cannot connect to installation : {_configuration["SiccarService"]}");
                return -1;
            }
            else
            {
                Console.WriteLine($"Testing against installation : {_configuration["SiccarService"]}");

                // TODO: Now Get the Token.
                bearer = await authn.DeviceConnect();

                if (string.IsNullOrWhiteSpace(bearer))
                {
                    Console.WriteLine($"Test client cannot continue.");
                    return -1;
                }

                await baseClient.SetBearerAsync(bearer);
            }

            // SIMPLE TESTS

            Console.WriteLine("Running basic test...");

            Console.WriteLine("\t Wallet tests");
            var walletTest = new WalletTests(_serviceProvider, bearer);

            var w1 = walletTest.Go_Basic();
            Console.WriteLine($"\t completed : {w1.Milliseconds} ms");

            walletTest.Dispose();

            Console.WriteLine("\t Register tests");
            var registerTest = new RegisterTests(_serviceProvider, bearer);

            var r1 = registerTest.Go_Basic();

            Console.WriteLine("Running PingPong test...");

            int rounds = 10;
            Console.WriteLine($"\t PingPong tests - {rounds}");
            var pingpong = new PingPongTest(_serviceProvider, bearer);

            TimeSpan t1 = TimeSpan.FromSeconds(0);

            var bpTxId = await pingpong.SetupTest();
            if (!string.IsNullOrEmpty(bpTxId))
                t1 = await pingpong.Go_PingPong(rounds);

            Console.WriteLine($"\t PingPong tests completed in {t1}");

            // FINISHED
            Console.WriteLine("Completed");
            return 0;
        }

        private static IConfiguration SetupConfiguration()
        {
            string homeDir = (Environment.OSVersion.Platform == PlatformID.Unix ||
                                  Environment.OSVersion.Platform == PlatformID.MacOSX)
                                ? Environment.GetEnvironmentVariable("HOME")!
                                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%")!;
            string homepath = Path.Combine(homeDir, ".siccar");
            // Setup and checkfile
            string appSettingsPath = Path.Combine(homepath, "appsettings.json");


            var defaulConfig = new Dictionary<string, string>()
            {
               // {"SiccarService", "https://n0.siccar.dev/"},
                {"SiccarService", "https://localhost:8443/"},
                {"clientId", "siccar-admin-ui-client"},
                {"clientSecret", "secret"},
                {"Scope", "installation.admin tenant.admin"}
            };

            // get config - but for the moment just use default CLI Providers
            return new ConfigurationBuilder()
                        .AddInMemoryCollection(defaulConfig)
                        .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile(appSettingsPath, optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .Build();
        }
    }
}