using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hollastin.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var client = new HttpClient();

            const string email = "bob@le-magnifique.com", password = "}s>EWG@f4g;_v7nB";

            await CreateAccountAsync(client, email, password);

            var token = await GetTokenAsync(client, email, password);
            Console.WriteLine("Access token: {0}", token);
            Console.WriteLine();

            var resource = await GetResourceAsync(client, token);
            Console.WriteLine("API response: {0}", resource);

            Console.ReadLine();
        }

        public static async Task CreateAccountAsync(HttpClient client, string email, string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:58795/Account/Register")
            {
                Content = new StringContent(JsonSerializer.Serialize(new { email, password }), Encoding.UTF8, "application/json")
            };

            // Ignore 409 responses, as they indicate that the account already exists.
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                return;
            }

            response.EnsureSuccessStatusCode();
        }

        public static async Task<string> GetTokenAsync(HttpClient client, string email, string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:58795/connect/token");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = email,
                ["password"] = password
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
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:58795/api/message");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
