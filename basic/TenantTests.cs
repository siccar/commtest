using Microsoft.Extensions.DependencyInjection;
using Siccar.Common.ServiceClients;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;

namespace CommTest.basic
{

    public class TenantTests
    {
        IConfiguration _configuration;
        ITenantServiceClient _tenatServiceClient;

        public TenantTests(IServiceProvider serviceProvider)
        {
            _configuration = serviceProvider.GetRequiredService<IConfiguration>();  
            _tenatServiceClient = serviceProvider.GetService<ITenantServiceClient>();
            if (_tenatServiceClient == null)
                throw new Exception("Cannot instanciate service client [TenantServiceClient]");
        }

        // we will confirm a basic check as can we get the 
        public bool CanConnect(string connectEndpoint)
        {
            var client = new HttpClient();
            var disco = client.GetDiscoveryDocumentAsync(connectEndpoint).Result;
            return !disco.IsError;
        }

        public TimeSpan Go_Basic()
        {
            var tenantStopwatch = new Stopwatch();


            return tenantStopwatch.Elapsed;

        }

        public TimeSpan Go_Advanced()
        {
            var tenantStopwatch = new Stopwatch();


            return tenantStopwatch.Elapsed;

        }

        public TimeSpan Go_Load()
        {

            var tenantStopwatch = new Stopwatch();
            return tenantStopwatch.Elapsed;

        }
    }
}

