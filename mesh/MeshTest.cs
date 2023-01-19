using Microsoft.Extensions.DependencyInjection;
using Siccar.Common.ServiceClients;
using System.Collections.Concurrent;
using System.Diagnostics;
using Siccar.Application;
using Siccar.Platform;
using System.Text.Json;
using System.Diagnostics.Metrics;
using System.Collections.Specialized;

namespace CommTest.mesh
{


    public class MeshTest : IDisposable
    {
        /// <summary>
        /// Sets up a Ping Pong transaction
        /// </summary>

        IBlueprintServiceClient _blueprintServiceClient;
        WalletServiceClient _walletServiceClient;
        RegisterServiceClient _registerServiceClient;
        ActionServiceClient _actionServiceClient;

        private string registerId = String.Empty;
        private string blueprintId = String.Empty;
        private string myWallet = string.Empty;
        private string nextWallet = string.Empty;
        private List<string> testParticipants = new List<string>();
        private Blueprint testBlueprint = new Blueprint();

        private static Random random = new Random();
        private int scaleSize = 0;
        private int _ballast = 0;
        private int rndFactor = 2; // choice of 2 will give balance, higer values wille skew bias

        private int executedRounds = 0;
        private int _cycles = 1;
        private DateTime _clock = DateTime.Now;
        private bool isSetup = false;
        private bool isStartingNode = false;
        private Siccar.Application.Action? startAction = null;

        public MeshTest(IServiceProvider serviceProvider, string bearer)
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

        public void Dispose()
        {
            throw new NotImplementedException();
        }


        public async Task<bool> Setup_Test(string MyWallet, string Register, string BlueprintId)
        {
            myWallet = MyWallet;
            registerId = Register;
            blueprintId = BlueprintId;

            var testBlueprints = await _blueprintServiceClient.GetAllPublished(Register);
            testBlueprint = testBlueprints.Last();

            isSetup = true;

            return true;
        }

        // run as a single thread per instance
        public async Task<TimeSpan> Run_Test(int node, int cycles, int ballast, int scale = 0)
        {
            scaleSize = scale;
            _ballast= ballast;
            _cycles = cycles;
            var testStopwatch = new Stopwatch();
            testStopwatch.Start();

            if (!isSetup)
                throw new Exception("Please setup the test environment first.");

            // workout the next wallet address from the blueprint
            int nextPart = node % testBlueprint.Participants.Count;

            nextWallet = testBlueprint.Participants[nextPart].WalletAddress;

            // setup comms


            _actionServiceClient.OnConfirmed += ProcessEvent;

            await _actionServiceClient.StartEvents();
            await _actionServiceClient.SubscribeWallet(myWallet);

            _clock = DateTime.Now;

            if (node == 1)
            {
                isStartingNode = true;
                await StartActions();
            }
            else
            {
                // we might already have a transaction waiting.. if so deal with it
                var unTxs = await _walletServiceClient.GetWalletTransactions(myWallet);
                Console.WriteLine($"Processing latest actions ... {unTxs.Count} ");
                if (unTxs.Any())
                {

                    var act2 = unTxs.Last();

                }
                Console.WriteLine("Awaiting incoming data...");
                //_actionServiceClient.ReceiveAction
            }

            Console.WriteLine("# Press a key to exit.");
            Console.ReadKey(true);


            testStopwatch.Stop();
            return testStopwatch.Elapsed;
        }

        public async Task ProcessEvent(TransactionConfirmed txData)
        {

            var eventStart = DateTime.Now;

                // check if its mine, if so ignore
            if (txData.Sender == myWallet)
            {  return; }

            if (executedRounds >= _cycles)
            {
                executedRounds = 0;
                Console.WriteLine("Instance Completed.");
                if (isStartingNode)
                {
                    await StartActions();
                }
                return;
            }

            var oldBallast = int.Parse(txData.MetaData.TrackingData["ballast"]);

            try
            {
                var nextAction = await _actionServiceClient.GetAction(txData.ToWallets.First(), txData.MetaData.RegisterId, txData.TransactionId, aggregatePreviousTransactionData: false);

                int newSize = oldBallast + scaleSize;
                ActionSubmission actionSubmit = new ActionSubmission()
                {
                    BlueprintId = blueprintId,
                    RegisterId = registerId,
                    WalletAddress = nextWallet,
                    PreviousTxId = txData.TransactionId,
                    Data = RandomEndorse(random.Next(rndFactor), newSize)
                };

                var tx = await _actionServiceClient.Submission(actionSubmit);
                Interlocked.Increment(ref executedRounds);

                var timespan = DateTime.Now - eventStart;
                var elapsed = _clock = DateTime.Now;
                _clock = DateTime.Now;

                Console.WriteLine($"Processed {executedRounds} in {timespan.Milliseconds} elasped since last {elapsed}: \n\tAction {nextAction.Title} on TxId : {tx.Id}");
               

                if (scaleSize > 0)
                    Console.WriteLine($"Ballast size : {newSize}");

            }
            catch (Exception er)
            {
                Console.WriteLine(er);
            }
        }

        private async Task StartActions()
        {
            Console.Write($"Getting starting action:");
            // we start by manually firing the get first action, ping
            while (startAction == null)
            {
                Console.Write($".");

                try
                {
                    var startActions = await _actionServiceClient.GetStartingActions(myWallet, registerId);
                    startAction = startActions.First();
                }
                catch (Exception er)
                {
                    Console.WriteLine(er.Message);
                }
                Thread.Sleep(500); // wait for the tx to arrive
            }

            ActionSubmission actionSubmit = new ActionSubmission()
            {
                BlueprintId = blueprintId,
                RegisterId = registerId,
                WalletAddress = myWallet,
                PreviousTxId = blueprintId,
                Data = RandomEndorse(1, _ballast)
            };

            Console.WriteLine($"\n\tConfirmed Start Action : {startAction.Description}");

            var tx = await _actionServiceClient.Submission(actionSubmit);
            Console.WriteLine($"Sending Inital Action {startAction.Title} on TxId : {tx.Id}");

        }

        /// <summary>
        /// RandomEndorse - generates payloads with a random Endorse Flag
        /// </summary>
        /// <param name="rnd"></param>
        /// <param name="ballast"></param>
        /// <returns></returns>

        private JsonDocument RandomEndorse(int rnd, int ballast = 0)
        {
            bool endorse = false;

            if (rnd > 0)
                endorse = true;

            string rndStr = "{ \"endorse\" : " + endorse.ToString().ToLower();


            if (ballast < 1)
                rndStr += ", \"ballast\" : 0 }";
            else
            {
                rndStr += $", \"ballast\" : \"{ballast}\" ";
                rndStr += ", \"textdata\" : \"" + CreateString(ballast) + "\" }";
            }

            JsonDocument doc = JsonDocument.Parse(rndStr);
            return doc;
        }

        internal static string CreateString(int stringLength)
        {
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            char[] chars = new char[stringLength];

            for (int i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[random.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }

    }
}
