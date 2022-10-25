﻿using MongoDB.Driver;
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
            this.Add(new Option<string>(new string[] { "--register", "-r" }, getDefaultValue: () => "", description: "ID of a Register to use"));
            this.Add(new Option<int>(new string[] { "--scale", "-s" }, getDefaultValue: () => 0, description: "An additional random text payload, size in characters"));

            _serviceProvider = services;

            Handler = CommandHandler.Create<int, string, int>(RunPingPong);
        }

        private async Task RunPingPong(int count, string register, int scale)
        {
            Console.WriteLine("Running PingPong test...");

            Console.WriteLine($"\t PingPong tests - {count}");

            if (scale > 0)
                Console.WriteLine($"\t Increasing the payload by {scale} characters");

            var pingpong = new PingPongTest(_serviceProvider, bearer);

            TimeSpan t1 = TimeSpan.FromSeconds(0);

            var bpTxId = await pingpong.SetupTest(register);
            if (!string.IsNullOrEmpty(bpTxId))
                t1 = await pingpong.Go_PingPong(count, scale);

            Console.WriteLine($"\t PingPong tests completed in {t1}");
        }
    }
}
