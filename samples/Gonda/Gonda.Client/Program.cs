using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Gonda.Server.Spec;
using Gonda.Client.Models;
using Grpc.Core;
using Grpc.Net.Client;

namespace Gonda.Client
{
    public static class Program
    {
        private const string AuthUrl = "https://localhost:5001";
        private const string ApiUrl = "http://localhost:5002";
        
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Attempting anonymous calls to gRPC service...\n");
            
            var anonymousClient = CreateAnonymousClient();
            
            await SayHello(anonymousClient);
            await SaySecret(anonymousClient);
            
            Console.WriteLine("Attempting authenticated calls to gRPC service...\n");
            
            await CreateUser();
            var tokens = await GetTokens();
            
            var authenticatedClient = CreateAuthenticatedClient(tokens.AccessToken);

            await SayHello(authenticatedClient);
            await SaySecret(authenticatedClient);
            
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static Greeter.GreeterClient CreateAnonymousClient()
        {
            var channel = GrpcChannel.ForAddress(ApiUrl);
            
            return new Greeter.GreeterClient(channel);
        }

        private static Greeter.GreeterClient CreateAuthenticatedClient(string token)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var opt = new GrpcChannelOptions()
            {
                HttpClient = httpClient, 
            };
            
            var channel = GrpcChannel.ForAddress(ApiUrl, opt);
            
            return new Greeter.GreeterClient(channel);
        }

        private static async Task SayHello(Greeter.GreeterClient client)
        {
            try
            {
                Console.WriteLine("Attempting to say hello to gRPC service...");

                var reply = await client.SayHelloAsync(new HelloRequest {Name = "GreeterClient"});

                Console.WriteLine("Response: " + reply.Message);
            }
            catch (RpcException ex)
            {
                Console.WriteLine("Response: " + ex.Status);
            }
            
            Console.WriteLine();
        }
        
        private static async Task SaySecret(Greeter.GreeterClient client)
        {
            try
            {
                Console.WriteLine("Attempting to tell a secret to gRPC service...");

                var reply = await client.SaySecretAsync(new SecretRequest {Secret = "Some thing"});

                Console.WriteLine("Response: " + reply.Message);
            }
            catch (RpcException ex)
            {
                Console.WriteLine("Response: " + ex.Status);
            }
            
            Console.WriteLine();
        }
        
        private static async Task CreateUser()
        {
            Console.WriteLine("Creating user...");
            
            var authClient = new HttpClient()
            {
                BaseAddress = new Uri(AuthUrl)
            };

            var request = new
            {
                Email = "test@email.com",
                Password = "Gonda~1",
                ConfirmPassword = "Gonda~1"
            };

            var response = await authClient.PostAsJsonAsync("Account/register", request);
            
            var responseBody = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Create user response... {(int)response.StatusCode} {response.ReasonPhrase} {responseBody}\n");
        }

        private static async Task<TokensResponse> GetTokens()
        {
            Console.WriteLine("Fetching auth token...");
            
            var authClient = new HttpClient()
            {
                BaseAddress = new Uri(AuthUrl)
            };

            var requestData = new Dictionary<string, string>()
            {
                ["username"] = "test@email.com",
                ["password"] = "Gonda~1",
                ["grant_type"] = "password",
                ["scope"] = "openid profile roles"
            };

            var content = new FormUrlEncodedContent(requestData);
            
            var response = await authClient.PostAsync("connect/token", content);
            
            var error = response.IsSuccessStatusCode 
                ? string.Empty 
                : await response.Content.ReadAsStringAsync();
            
            var tokens = response.IsSuccessStatusCode 
                ? await response.Content.ReadFromJsonAsync<TokensResponse>()
                : null;

            Console.WriteLine($"Token response... {(int)response.StatusCode} {response.ReasonPhrase} {error}\n");
            
            return tokens;
        }
    }
}