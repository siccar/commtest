using System;
using System.Collections.Generic;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Siccar.Application;
using Siccar.Platform;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Siccar.Common.ServiceClients;
using Action = Siccar.Application.Action;

namespace CommTest.mesh
{
    public class MeshSetupCommand : Command
    {
        IServiceProvider serviceProvider;
        string bearer = "";
        IBlueprintServiceClient _blueprintServiceClient;
        WalletServiceClient _walletServiceClient;
        RegisterServiceClient _registerServiceClient;
        ActionServiceClient _actionServiceClient;
        private List<Participant> testParticipants = new List<Participant>();

        public MeshSetupCommand(string action, IServiceProvider services) : base(action)
        {
            this.Name = "setup";
            this.Description = "Sets up a simple action test that excersizes the stack.";

            this.Add(new Option<int>(new string[] { "--participants", "-p" }, getDefaultValue: () => 2, description: "Number Participants to create"));
            this.Add(new Option<string>(new string[] { "--register", "-r" }, getDefaultValue: () => "", description: "ID of a Register to use"));
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


            Handler = CommandHandler.Create<int, string>(SetupMesh);
        }

        private async Task SetupMesh(int participants, string register)
        {
            Console.WriteLine("Creating the Mesh test...");
            Register useRegister;
            bool reuse = false;

            if (string.IsNullOrEmpty(register))
            {
                // Create a register
                useRegister = _registerServiceClient.CreateRegister(
                    new Register()
                    {
                        Advertise = false,
                        Name = "Mesh Test : " + DateTime.Now.ToString()
                    }).Result;

                Console.WriteLine($"Created new Register : {useRegister.Id}");
            }
            else
            {
                useRegister = await _registerServiceClient.GetRegister(register);
                if (useRegister != null)
                {
                    Console.WriteLine($"Reusing Register : {useRegister.Id}");
                    reuse = true;
                }
                else
                    throw new Exception("Register Does Not Exist");

            }

            // create Wallets/Participants

            Console.WriteLine($"Creating the Participants : {participants}");

            for (int i = 1; i <= participants; i++)
            {
                Participant newParticipant = new Participant();
                newParticipant.Name = $"Mesh Test Participant {i}";
                newParticipant.Organisation = "Mesh Test Org";
                Wallet newWallet = await _walletServiceClient.CreateWallet($"Test Wallet [{i}]", $"A Mesh Test Wallet Number : {i}");
                newParticipant.WalletAddress = newWallet.Address;
                testParticipants.Add(newParticipant);
                Console.WriteLine($"Created new Participant [{i-1}] : {testParticipants[i-1].WalletAddress}");
            }

            var bpBuilder = new BuildMeshBlueprint();
            var blueprint = bpBuilder.Build(testParticipants);

            // is it worth writing a debug copy?

            // what no blueprint service client
            RawTransaction bpTxId;
            try
            {
                bpTxId = await _blueprintServiceClient.PublishBlueprint(testParticipants[0].WalletAddress, useRegister.Id, blueprint);

                Console.WriteLine($"Test initialized,  for each participant 'n' execute an instance :");

                int i = 1;
                foreach(var w in testParticipants)
                {
                    Console.WriteLine($"\n\t ./CommTest.exe mesh run {i++} {w.WalletAddress} {useRegister.Id} {bpTxId.Id}\n");
                }
                
            }
            catch(Exception er)
            {
                Console.WriteLine($"Test initialized Failed {er.Message}");
            }
        }

  
    }
}
