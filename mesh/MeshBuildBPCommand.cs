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
    public class MeshBuildBPCommand : Command
    {

        private List<Participant> testParticipants = new List<Participant>();

        public MeshBuildBPCommand(string action, IServiceProvider services) : base(action)
        {
            this.Name = "build";
            this.Description = "Builds and saves the Mesh blueprint, mostly just for testing...";

            this.Add(new Option<int>(new string[] { "--participants", "-p" }, getDefaultValue: () => 2, description: "Number Participants to create"));

            Handler = CommandHandler.Create<int>(SetupMesh);
        }

        private async Task SetupMesh(int participants)
        {
            for (int i = 1; i <= participants; i++)
            {
                Participant participant = new Participant();
                participant.Id = Guid.NewGuid().ToString();
                participant.Name = $"Test Participant [{i}]";
                participant.Organisation = "Mesh test Org";

                Wallet newWallet = new Wallet()
                {   
                    Name = $"Test Wallet [{i}]",    
                    Address = $"000000000000000{i}"
                };

                participant.WalletAddress = newWallet.Address;

               // await _walletServiceClient.CreateWallet($"Test Wallet [{i}]", $"A Mesh Test Wallet Number : {i}");
                testParticipants.Add(participant);
                Console.WriteLine($"Created new Participant [{i}] : {testParticipants[i-1].WalletAddress}");
            }
            var bpBuilder = new BuildMeshBlueprint();
            var blueprint = bpBuilder.Build(testParticipants);

            JsonSerializerOptions jopts = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            jopts.WriteIndented = true;
            string bp =  JsonSerializer.Serialize<Blueprint>(blueprint, jopts);

            Console.WriteLine(bp);  
        }
    }
}