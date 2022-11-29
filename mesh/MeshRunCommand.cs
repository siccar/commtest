using System;
using System.Collections.Generic;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Siccar.Application;
using Microsoft.Extensions.DependencyInjection;
using Siccar.Common.ServiceClients;

namespace CommTest.mesh
{
    public class MeshRunCommand : Command
    {
        IServiceProvider serviceProvider;
        IBlueprintServiceClient _blueprintServiceClient;
        WalletServiceClient _walletServiceClient;
        RegisterServiceClient _registerServiceClient;
        ActionServiceClient _actionServiceClient;
        string bearer = "";

        public MeshRunCommand(string action, IServiceProvider services) : base(action)
        {
            this.Name = "run";
            this.Description = "Runs a simple action test that excersizes the stack.";

            this.Add(new Argument<int>("nodeId", getDefaultValue: () => 1, description: "ID of a Participant to use"));
            this.Add(new Argument<string>("walletAddress", getDefaultValue: () => "", description: "Wallet to use"));
            this.Add(new Argument<string>("register", getDefaultValue: () => "", description: "Register ID to use"));
            this.Add(new Argument<string>("transaction", getDefaultValue: () => "", description: "Blueprint Transaction Id"));

            //this.Add(new Option<int>(new string[] { "--scale", "-s" }, getDefaultValue: () => 0, description: "An additional random text payload, size in characters"));
            this.Add(new Option<int>(new string[] { "--ballast", "-b" }, getDefaultValue: () => 0, description: "An initial payload size"));

            serviceProvider = services;

            _actionServiceClient = (ActionServiceClient)serviceProvider.GetService<IActionServiceClient>();
            if (_actionServiceClient == null)
                throw new Exception("Cannot instanciate service client [ActionServiceClient]");
            _blueprintServiceClient = (IBlueprintServiceClient)serviceProvider.GetService<IBlueprintServiceClient>();
            if (_blueprintServiceClient == null)
                throw new Exception("Cannot instanciate service client [BlueprintServiceClient]");
            _walletServiceClient = (WalletServiceClient)serviceProvider.GetService<IWalletServiceClient>();
            if (_walletServiceClient == null)
                throw new Exception("Cannot instanciate service client [WalletServiceClient]");
            _registerServiceClient = (RegisterServiceClient)serviceProvider.GetService<IRegisterServiceClient>();
            if (_registerServiceClient == null)
                throw new Exception("Cannot instanciate service client [RegisterServiceClient]");


            Handler = CommandHandler.Create<int, string, string, string, int>(RunMesh);
        }

        private async Task RunMesh(int nodeId, string walletAddress, string register, string blueprintId, int ballast)
        {
            Console.WriteLine("Running Mesh test...");

            Console.WriteLine("Checking Register for Blueprint...");

            var bps = await _blueprintServiceClient.GetAllPublished(walletAddress, register);
            
            Console.WriteLine($"\t Starting payload is {ballast} characters");


            var thisTest = new MeshTest(serviceProvider, bearer);
            thisTest.Setup_Test(walletAddress, register, blueprintId);

            TimeSpan t1 = TimeSpan.FromSeconds(0);

            t1 = await thisTest.Run_Test(nodeId, ballast);

            Console.WriteLine($"\t Mesh tests completed in {t1}");
        }
    }
}
