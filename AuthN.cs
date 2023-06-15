using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Siccar.Common;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Siccar.Common.ServiceClients;
using IdentityModel;
using IdentityModel.Client;
using System.Net.Http;

namespace CommTest.basic
{
    public class AuthN
    {
        public ClaimsPrincipal claimsUser;
        public IConfiguration configuration;
        //private UserAuthentication userAuthentication;
        //private string serviceUri = "";
        private DiscoveryDocumentResponse disco;
        private HttpClient client;

        public AuthN(IConfiguration config)
        {
            configuration = config;
            client = new HttpClient();
            claimsUser = new ClaimsPrincipal();
            disco = client.GetDiscoveryDocumentAsync(configuration["SiccarService"]).Result;
        }

        public async Task<string> Login(string uri = "")
        {
            if (string.IsNullOrEmpty(uri))
                uri = configuration["SiccarService"];

            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return "";
            }
            var devConnect = disco.DeviceAuthorizationEndpoint ?? "connect/authorize";

            Console.WriteLine("Authenticating as Device");
            var result = await client.RequestDeviceAuthorizationAsync(new DeviceAuthorizationRequest
            {
                Address = devConnect,
                ClientId = configuration["clientId"],
                ClientSecret = configuration["clientSecret"],
                Scope = configuration["scope"]
            });

            if (result.IsError)
            {
                Console.WriteLine($"Failed : {result.Error}");
                return "";
            }

            Console.WriteLine($"user code   : {result.UserCode}");
            Console.WriteLine($"device code : {result.DeviceCode}");
            Console.WriteLine($"URL         : {result.VerificationUri}");
            Console.WriteLine($"Complete URL: {result.VerificationUriComplete}");

            var tokenResponse = await RequestTokenAsync(result);
            Console.WriteLine($"Access Token: {tokenResponse.AccessToken}");
            return tokenResponse.AccessToken;

        }

        public async Task<string> LoginUser(string uri = "")
        {
            Console.WriteLine("Authenticating as User");
            if (string.IsNullOrEmpty(uri))
                uri = configuration["SiccarService"];

            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return "";
            }
            var devConnect = disco.DeviceAuthorizationEndpoint ?? "connect/authorize";

            Console.WriteLine("Authorization Request");
            var result = await client.RequestAuthorizationCodeTokenAsync(new AuthorizationCodeTokenRequest
            {
                Address = devConnect,
                ClientId = configuration["clientId"],
                ClientSecret = configuration["clientSecret"]
            });

            if (result.IsError)
            {
                Console.WriteLine($"Failed : {result.Error}");
                return "";
            }

            Console.WriteLine($"access token   : {result.AccessToken}");


            return result.AccessToken;

        }

        public async Task<string> DeviceConnect()
        {
            var clientId = configuration["clientId"];
            var clientSecret = configuration["clientSecret"];
            var scope = configuration["scope"];

            Console.WriteLine("Authenticating using Client Credentials");
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = scope

            }); 

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return "";
            }
            return tokenResponse.AccessToken;
        }

        private async Task<TokenResponse> RequestTokenAsync(DeviceAuthorizationResponse authorizeResponse)
        {
            Console.WriteLine($"Getting Access Token: ...");
            while (true)
            {
                var response = await client.RequestDeviceTokenAsync(new DeviceTokenRequest
                {
                    Address = disco.TokenEndpoint,
                    ClientId = "cli",
                    DeviceCode = authorizeResponse.DeviceCode
                });

                if (response.IsError)
                {
                    if (response.Error == OidcConstants.TokenErrors.AuthorizationPending || response.Error == OidcConstants.TokenErrors.SlowDown)
                    {
                        Console.WriteLine($"{response.Error}...waiting.");
                        await Task.Delay(authorizeResponse.Interval * 1000);
                    }
                    else
                    {
                        throw new Exception(response.Error);
                    }
                }
                else
                {
                    return response;
                }
            }
        }
    }
}
