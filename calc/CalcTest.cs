using Microsoft.Extensions.DependencyInjection;
using Siccar.Common.ServiceClients;
using Siccar.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommTest.calc
{
    internal class CalcTest
    {
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
        public CalcTest(IServiceProvider serviceProvider, string bearer)
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

        public async Task<string> SetupTest(string register, int threads)
        {
            return ("txId");
        }
        public TimeSpan Go_Calc(int itterations)
        {

            return TimeSpan.Zero;
        }
    }
}
