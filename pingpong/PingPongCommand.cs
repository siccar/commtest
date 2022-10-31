using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;


namespace CommTest.pingpong
{
    public class PingPongCommand : Command
    {
        IServiceProvider _serviceProvider;
        string bearer = "";

        public PingPongCommand(string action, IServiceProvider services) : base(action)
        {
            this.Name = "pingpong";
            this.Description = "Runs a simple action test that excersizes the stack.";

            this.Add(new Option<int>(new string[] { "--count", "-c" }, getDefaultValue: () => 10, description: "Number of pingpong rounds"));
            this.Add(new Option<int>(new string[] { "--threads", "-t" }, getDefaultValue: () => 2, description: "Number wallet threads to run in parallel"));
            this.Add(new Option<string>(new string[] { "--register", "-r" }, getDefaultValue: () => "", description: "ID of a Register to use"));
            this.Add(new Option<int>(new string[] { "--scale", "-s" }, getDefaultValue: () => 0, description: "An additional random text payload, size in characters"));
            this.Add(new Option<int>(new string[] { "--ballast", "-b" }, getDefaultValue: () => 0, description: "An initial payload size"));

            _serviceProvider = services;

            Handler = CommandHandler.Create<int, int, string, int, int>(RunPingPong);
        }

        private async Task RunPingPong(int count, int threads, string register, int ballast, int scale)
        {
            Console.WriteLine("Running PingPong test...");

            Console.WriteLine($"\t Test loops : {count}");
            Console.WriteLine($"\t Starting payload is {ballast} characters");
            Console.WriteLine($"\t Increasing the payload by {scale} characters");

            var pingpong = new PingPongTest(_serviceProvider, bearer);

            TimeSpan t1 = TimeSpan.FromSeconds(0);

            var bpTxId = await pingpong.SetupTest(register, threads);
            if (!string.IsNullOrEmpty(bpTxId))
                t1 = await pingpong.Go_PingPong(count, ballast, scale);

            Console.WriteLine($"\t PingPong tests completed in {t1}");
        }
    }
}
