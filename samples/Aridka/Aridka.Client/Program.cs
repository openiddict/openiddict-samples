﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Aridka.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var client = new HttpClient();

            try
            {
                var token = await GetTokenAsync(client);
                Console.WriteLine("Access token: {0}", token);
                Console.WriteLine();

                var resource = await GetResourceAsync(client, token);
                Console.WriteLine("API response: {0}", resource);
                Console.ReadLine();
            }
            catch (HttpRequestException exception)
            {
                var builder = new StringBuilder();
                builder.AppendLine("+++++++++++++++++++++");
                builder.AppendLine(exception.Message);
                builder.AppendLine(exception.InnerException?.Message);
                builder.AppendLine("Make sure you started the authorization server.");
                builder.AppendLine("+++++++++++++++++++++");
                Console.WriteLine(builder.ToString());
            }
        }

        public static async Task<string> GetTokenAsync(HttpClient client)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:52698/connect/token");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = "console",
                ["client_secret"] = "388D45FA-B36B-4988-BA59-B187D329C207"
            });

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            response.EnsureSuccessStatusCode();


            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = document.RootElement;
            if (root.TryGetProperty("error", out var error) && error.GetString() != null)
            {
                throw new InvalidOperationException("An error occurred while retrieving an access token.");
            }

            return root.GetProperty("access_token").GetString();
        }

        public static async Task<string> GetResourceAsync(HttpClient client, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:52698/api/message");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
