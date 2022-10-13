using Microsoft.Extensions.DependencyInjection;
using Siccar.Common.ServiceClients;
using System.Diagnostics;
using Siccar.Application;
using Siccar.Platform;
using System.Text.Json;

namespace CommTest.basic
{
    public class PingPongTest : IDisposable
    {
        /// <summary>
        /// Sets up a Ping Pong transaction
        /// </summary>

        IBlueprintServiceClient _blueprintServiceClient;
        WalletServiceClient _walletServiceClient;
        RegisterServiceClient _registerServiceClient;
        ActionServiceClient _actionServiceClient;

        private string registerId = String.Empty;
        private string pingWallet = String.Empty;
        private string pongWallet = String.Empty;
        private string blueprintId = String.Empty;

        private Random random = new Random();
        private int rndFactor = 2; // choice of 2 will give balance, higer values wille skew bias

        private bool isSetup = false;

        public PingPongTest(IServiceProvider serviceProvider, string bearer)
        {
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

        }

        // check we have everything we need
        public async Task<string> SetupTest()
        {
            // Create a register
            var register = _registerServiceClient.CreateRegister(
                new Register()
                {
                    Advertise = false,
                    Name = "ping pong register"
                }).Result;

            registerId = register.Id;
            Console.WriteLine($"Created new Register : {registerId}");

            // create P1 & P2
            var wallet1 = await _walletServiceClient.CreateWallet("Ping Wallet");
            Console.WriteLine($"Created new Ping Wallet : {wallet1.Address}");
            pingWallet = wallet1.Address;

            var wallet2 = await _walletServiceClient.CreateWallet("Pong Wallet");
            Console.WriteLine($"Created new Pong Wallet : {wallet2.Address}");
            pongWallet = wallet2.Address;

            // Load and ammend the PingPong Blueprint, then publish it
            var strbase = File.ReadAllText("pingpong.json");
            var string1 = strbase.Replace("{{walletAddress1}}", pingWallet);
            var string2 = string1.Replace("{{walletAddress2}}", pongWallet);

            JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web);

            var blueprint = JsonSerializer.Deserialize<Blueprint>(string2, serializerOptions);

            // is it worth writing a debug copy?

            // what no blueprint service client
            var bpTxId = await _blueprintServiceClient.PublishBlueprint(pingWallet, registerId, blueprint);


            Console.WriteLine($"Blueprint created and published, TXId  : {bpTxId.Id}");

            blueprintId = bpTxId.Id;

            isSetup = true;
            return bpTxId.Id;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task<TimeSpan> Go_PingPong(int rounds)
        {
            var pingpongStopwatch = new Stopwatch();
            pingpongStopwatch.Start();

            Siccar.Application.Action? startAction = null;
            Console.Write($"Getting starting action:");
            // we start by manually firing the get first action, ping
            while (startAction == null)
            {
                Console.Write($".");

                try
                {
                    startAction = await _actionServiceClient.GetAction(pingWallet, registerId, blueprintId);
                }
                catch (Exception er)
                {

                }
                Thread.Sleep(500); // wait for the tx to arrive
            }
            

            ActionSubmission actionSubmit = new ActionSubmission()
            {
                BlueprintId = blueprintId,
                RegisterId = registerId,
                WalletAddress = pingWallet,
                PreviousTxId = blueprintId,
                Data = RandomEndorse(1)
            };

            await _actionServiceClient.StartEvents();

            _actionServiceClient.OnConfirmed += ProcessEvent;

            await _actionServiceClient.SubscribeWallet(pingWallet);
            await _actionServiceClient.SubscribeWallet(pongWallet);


            Console.WriteLine($"\n\tConfirmed Start Action : {startAction.Description}");

            var tx = await _actionServiceClient.Submission(actionSubmit);
            Console.WriteLine($"Processed Action {startAction.Title} on TxId : {tx.Id}");

            Console.WriteLine(">>>>>> Press key to exit <<<<<<");
            var input = Console.ReadKey();

            pingpongStopwatch.Stop();
            return pingpongStopwatch.Elapsed;
        }

        public async Task ProcessEvent(TransactionConfirmed txData)
        {
            var stateWallet = pongWallet;

            if (txData.Sender == pongWallet)
                stateWallet = pingWallet;

            try
            {
                var nextAction = await _actionServiceClient.GetAction(txData.ToWallets.First(), txData.MetaData.RegisterId, txData.TransactionId);

                ActionSubmission actionSubmit = new ActionSubmission()
                {
                    BlueprintId = blueprintId,
                    RegisterId = registerId,
                    WalletAddress = stateWallet,
                    PreviousTxId = txData.TransactionId,
                    Data = RandomEndorse(random.Next(rndFactor))
                };

                var tx = await _actionServiceClient.Submission(actionSubmit);

                Console.WriteLine($"Processed Action {nextAction.Title} on TxId : {tx.Id}");
            }
            catch (Exception er)
            {
                Console.WriteLine(er);
            }
        }
        private JsonDocument RandomEndorse(int rnd)
        {
            bool endorse = false;

            if (rnd > 0)
                endorse = true;

            string rndStr = "{ \"endorse\" : " + endorse.ToString().ToLower() + " }";

            return JsonDocument.Parse(rndStr);
        }

    }
}
