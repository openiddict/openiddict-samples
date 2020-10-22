using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using System.Threading.Tasks;
using OpenIddict.Abstractions;

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
            var response = await client.PostAsJsonAsync("https://localhost:44360/Account/Register", new { email, password });

            // Ignore 409 responses, as they indicate that the account already exists.
            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                return;
            }

            response.EnsureSuccessStatusCode();
        }

        public static async Task<string> GetTokenAsync(HttpClient client, string email, string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://localhost:44360/connect/token");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = email,
                ["password"] = password
            });

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            var payload = await response.Content.ReadFromJsonAsync<OpenIddictResponse>();

            if (!string.IsNullOrEmpty(payload.Error))
            {
                throw new InvalidOperationException("An error occurred while retrieving an access token.");
            }

            return payload.AccessToken;
        }

        public static async Task<string> GetResourceAsync(HttpClient client, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44360/api/message");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
