using Microsoft.Extensions.DependencyInjection;
using Siccar.Common.ServiceClients;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CommTest.basic
{
    public class BlueprintTests : IDisposable
    {
        BlueprintServiceClient _blueprintServiceClient;
        private TestData testData = new TestData();


        public BlueprintTests(IServiceProvider serviceProvider, string bearer)
        {
            _blueprintServiceClient = serviceProvider.GetService<BlueprintServiceClient>();
            if (_blueprintServiceClient == null)
                throw new Exception("Cannot instanciate service client [BlueprintServiceClient]");
            
        }

        public TimeSpan Go_Basic()
        {
            var walletStopwatch = new Stopwatch();


            return walletStopwatch.Elapsed;
        }


            public void Dispose()
        {
            throw new NotImplementedException();
        }
    }





}
