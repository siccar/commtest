using Microsoft.Extensions.DependencyInjection;
using Siccar.Common.ServiceClients;
using System.Collections.Concurrent;
using System.Diagnostics;
using Siccar.Application;
using Siccar.Platform;
using System.Text.Json;
using System.Diagnostics.Metrics;
using System.Collections.Specialized;

namespace CommTest.pingpong
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
        private string blueprintId = String.Empty;
        private List<Wallet> testWallets = new List<Wallet>();

        private static Random random = new Random();
        private int scaleSize = 0;
        private int rndFactor = 2; // choice of 2 will give balance, higer values wille skew bias

        private int executedRounds = 1;
        private bool isSetup = false;
        private string _bearer = "";

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
            _bearer = bearer;
        }

        // check we have everything we need
        public async Task<string> SetupTest(string useregister, int threads)
        {
            bool reuse = false;
            Console.WriteLine("Initialising Register:");

            if (useregister.Length < 1)
            {
                // Create a register
                var register = _registerServiceClient.CreateRegister(
                    new Register()
                    {
                        Advertise = false,
                        Name = "PingPong Test : " + DateTime.Now.ToString()
                    }).Result;

                registerId = register.Id;
                Console.WriteLine($"Created new Register : {registerId}");
            }
            else
            {
                var reg = await _registerServiceClient.GetRegister(useregister);
                if (reg != null)
                {
                    registerId = useregister;
                    Console.WriteLine($"Re Using Register : {useregister}");
                    reuse = true;
                }
                else
                    throw new Exception("Register Does Not Exist");

            }
            // create Wallets
            Console.WriteLine("Initialising Wallets:");
            for (int i = 0; i < threads; i++)
            {
                Wallet newWallet = await _walletServiceClient.CreateWallet($"Test Wallet [{i}]");
                testWallets.Add(newWallet);
                Console.WriteLine($"Created new Wallet[{i}] : {testWallets[i].Address}");
            }
            
            var blueprint = BuildBlueprint("pingpong/pingpong.json", testWallets);
            
            // is it worth writing a debug copy?

            // what no blueprint service client
            RawTransaction bpTxId;
            if (reuse)
            {
                Console.WriteLine($"Not currently reusing Blueprint");
            }
            if (testWallets.Count<1)
            {
                throw new Exception("Must run a number of threads/wallets");
            }
            bpTxId = await _blueprintServiceClient.PublishBlueprint(testWallets[0].Address, registerId, blueprint);

            Console.WriteLine($"Blueprint created and published, TXId  : {bpTxId.Id}");

            blueprintId = bpTxId.Id;

            isSetup = true;
            return bpTxId.Id;
        }

        /// <summary>
        /// Build Blueprint
        /// 
        /// The idea is to load any Blueprint template and replace the wallet addresses with those in the generated list
        /// </summary>
        /// <param name="template"></param>
        /// <param name="testWallets"></param>
        /// <returns></returns>
        private Blueprint BuildBlueprint(string template, List<Wallet> testWallets)
        {
            // Load and ammend the PingPong Blueprint, then publish it
            var strbase = File.ReadAllText("pingpong/pingpong.json");

            for (int i = 0; i < testWallets.Count; i++)
            {
                strbase = strbase.Replace($"@@walletAddress{i}@@", testWallets[i].Address);
            }

            JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web);

            return JsonSerializer.Deserialize<Blueprint>(strbase, serializerOptions);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task<TimeSpan> Go_PingPong(int rounds, int ballast, int scale)
        {
            scaleSize = scale;
            var pingpongStopwatch = new Stopwatch();
            pingpongStopwatch.Start();

            if (!isSetup)
                throw new Exception("Please setup the test environment first.");

            Siccar.Application.Action? startAction = null;
            Console.Write($"Getting starting action:");
            // we start by manually firing the get first action, ping
            while (startAction == null)
            {
                Console.Write($".");

                try
                {
                    startAction = await _actionServiceClient.GetAction(testWallets[0].Address, registerId, blueprintId);
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
                WalletAddress = testWallets[0].Address,
                PreviousTxId = blueprintId,
                Data = RandomEndorse(1, ballast)
            };

            await _actionServiceClient.SetBearerAsync(_bearer);
            await _actionServiceClient.StartEvents();

            _actionServiceClient.OnConfirmed += ProcessEvent;

            await _actionServiceClient.SubscribeWallet(testWallets[0].Address);
            await _actionServiceClient.SubscribeWallet(testWallets[1].Address);


            Console.WriteLine($"\n\tConfirmed Start Action : {startAction.Description}");

            var tx = await _actionServiceClient.Submission(actionSubmit);
            Console.WriteLine($"Sending Inital Action {startAction.Title} on TxId : {tx.Id}");

            if (rounds != 0)
            {
                while (executedRounds <= rounds)
                {
                    Thread.Sleep(250);
                }
            }
            else
            {
                Console.ReadKey(true);
            }

            pingpongStopwatch.Stop();
            return pingpongStopwatch.Elapsed;
        }

        public async Task ProcessEvent(TransactionConfirmed txData)
        {

            var thisWallet = txData.ToWallets.First();

            var oldBallast = int.Parse(txData.MetaData.TrackingData["ballast"]);

            try
            {
                var nextAction = await _actionServiceClient.GetAction(txData.ToWallets.First(), txData.MetaData.RegisterId, txData.TransactionId);

                int newSize = oldBallast + scaleSize;
                ActionSubmission actionSubmit = new ActionSubmission()
                {
                    BlueprintId = blueprintId,
                    RegisterId = registerId,
                    WalletAddress = thisWallet,
                    PreviousTxId = txData.TransactionId,
                    Data = RandomEndorse(random.Next(rndFactor), newSize)
                };

                var tx = await _actionServiceClient.Submission(actionSubmit);

                Console.WriteLine($"Processed {executedRounds} Action {nextAction.Title} on TxId : {tx.Id}");
                Interlocked.Increment(ref executedRounds);

                if (scaleSize > 0)
                    Console.WriteLine($"Ballast size : {newSize}");

            }
            catch (Exception er)
            {
                Console.WriteLine(er);
            }
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
