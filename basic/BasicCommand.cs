using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;


namespace CommTest.basic
{
    public class BasicCommand : Command
    {
        IServiceProvider _serviceProvider;
        string bearer = "";

        public BasicCommand(string action, IServiceProvider services) : base(action)
        {

            _serviceProvider = services;

        }

        private async Task RunBasic()
        {
            Console.WriteLine("Running basic test...");

            Console.WriteLine("\t Wallet tests");
            var walletTest = new WalletTests(_serviceProvider, bearer);

            var w1 = walletTest.Go_Basic();
            Console.WriteLine($"\t completed : {w1.Milliseconds} ms");

            walletTest.Dispose();

            Console.WriteLine("\t Register tests");
            var registerTest = new RegisterTests(_serviceProvider, bearer);

            var r1 = registerTest.Go_Basic();
        }
    }
}
