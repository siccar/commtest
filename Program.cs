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
using static Google.Rpc.Context.AttributeContext.Types;
using Google.Api;
using Microsoft.ApplicationInsights;
using System.CommandLine;
using Microsoft.ApplicationInsights.Extensibility;

namespace CommTest.basic
{
    class Program
    {
        private static IConfiguration _configuration;
        static string bearer = "";

        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("Siccar System Checker v2.0");

            TelemetryConfiguration.Active.InstrumentationKey = "69f3e190-025e-435c-868c-0eaf7dc5412a"; 

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

            int successorfail = await Connect(_serviceProvider);

            if (successorfail > 0)
                return successorfail;

            // Create a Root Command (which only runs if there are no subcommands) and add SubCommands
            var rootCommand = new RootCommand("Siccar Communications and Test Tool");

            // making heirarchical command structure
            rootCommand.AddCommand(new Command("basic", "simple service tests")
            {
                new basic.BasicCommand("wallet", _serviceProvider),
            });
            rootCommand.AddCommand(new Command("mesh", "Mesh transaction test")
            {
                new mesh.MeshSetupCommand("setup", _host.Services),
                new mesh.MeshRunCommand("run", _host.Services),
                //new mesh.MeshBuildBPCommand("build", _host.Services)
            });

            rootCommand.AddCommand(new pingpong.PingPongCommand("pingpong", _serviceProvider, bearer));
            return await rootCommand.InvokeAsync(args);
        }

        private static async Task<int> Connect(IServiceProvider serviceProvider)
        {
            Console.WriteLine("Examining configuration and connectivity");
            var authn = new AuthN(_configuration);

            var baseClient = serviceProvider.GetService<SiccarBaseClient>();

            var tenantTests = new TenantTests(serviceProvider);
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
                // bearer = await authn.Login();
                if (string.IsNullOrWhiteSpace(bearer))
                {
                    Console.WriteLine($"Test client cannot continue.");
                    return 1;
                }

                await baseClient.SetBearerAsync(bearer);

                Console.WriteLine($"Using Token : {bearer}");

                return 0;
            }
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
                {"Scope", "installation.admin tenant.admin register.creator wallet.user"}
            };

            // get config - but for the moment just use default CLI Providers
            return new ConfigurationBuilder()
                        .AddInMemoryCollection(defaulConfig)
                        .AddJsonFile(appSettingsPath, optional: true, reloadOnChange: true)
                        .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .Build();
        }
    }
}