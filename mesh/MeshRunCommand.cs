using System;
using System.Collections.Generic;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommTest.mesh
{
    public class MeshRunCommand : Command
    {
        IServiceProvider _serviceProvider;
        string bearer = "";

        public MeshRunCommand(string action, IServiceProvider services) : base(action)
        {
            this.Name = "run";
            this.Description = "Runs a simple action test that excersizes the stack.";

            //this.Add(new Option<int>(new string[] { "--count", "-c" }, getDefaultValue: () => 10, description: "Number of pingpong rounds"));
            //this.Add(new Option<int>(new string[] { "--threads", "-t" }, getDefaultValue: () => 2, description: "Number wallet threads to run in parallel"));
            this.Add(new Argument<string>("register", getDefaultValue: () => "", description: "Register ID to use"));
            this.Add(new Argument<string>("transaction", getDefaultValue: () => "", description: "Blueprint Transaction Id"));
            this.Add(new Argument<int>("nodeId", getDefaultValue: () => 1, description: "ID of a Participant to use"));
            //this.Add(new Option<int>(new string[] { "--scale", "-s" }, getDefaultValue: () => 0, description: "An additional random text payload, size in characters"));
            this.Add(new Option<int>(new string[] { "--ballast", "-b" }, getDefaultValue: () => 0, description: "An initial payload size"));

            _serviceProvider = services;

            Handler = CommandHandler.Create<string, int>(RunMesh);
        }

        private async Task RunMesh( string register, int ballast)
        {
            Console.WriteLine("Running Mesh test...");

            Console.WriteLine("Checking Register for Blueprint...");


            Console.WriteLine($"\t Starting payload is {ballast} characters");


            var pingpong = new MeshTest(_serviceProvider, bearer);

            TimeSpan t1 = TimeSpan.FromSeconds(0);

            //var bpTxId = await pingpong.SetupTest(register, threads);
            //if (!string.IsNullOrEmpty(bpTxId))
            //    t1 = await pingpong.Go_PingPong(count, ballast, scale);

            Console.WriteLine($"\t Mesh tests completed in {t1}");
        }
    }
}
