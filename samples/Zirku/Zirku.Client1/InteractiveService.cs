using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OpenIddict.Client;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Abstractions.OpenIddictExceptions;

namespace Zirku.Client1;

public class InteractiveService : BackgroundService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly OpenIddictClientService _service;

    public InteractiveService(
        IHostApplicationLifetime lifetime,
        OpenIddictClientService service)
    {
        _lifetime = lifetime;
        _service = service;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Wait for the host to confirm that the application has started.
        var source = new TaskCompletionSource<bool>();
        using (_lifetime.ApplicationStarted.Register(static state => ((TaskCompletionSource<bool>) state!).SetResult(true), source))
        {
            await source.Task;
        }

        Console.WriteLine("Press any key to start the authentication process.");
        await Task.Run(Console.ReadKey).WaitAsync(stoppingToken);

        try
        {
            // Ask OpenIddict to initiate the authentication flow (typically, by starting the system browser).
            var result = await _service.ChallengeInteractivelyAsync(new()
            {
                AdditionalAuthorizationRequestParameters = new()
                {
                    ["hardcoded_identity_id"] = "1"
                },
                CancellationToken = stoppingToken
            });

            Console.WriteLine("System browser launched.");

            // Wait for the user to complete the authorization process.
            var response = await _service.AuthenticateInteractivelyAsync(new()
            {
                Nonce = result.Nonce
            });

            Console.WriteLine("Response from Api1: {0}", await GetResourceFromApi1Async(
                response.BackchannelAccessToken ?? response.FrontchannelAccessToken, stoppingToken));
            Console.WriteLine("Response from Api2: {0}", await GetResourceFromApi2Async(
                response.BackchannelAccessToken ?? response.FrontchannelAccessToken, stoppingToken));
        }

        catch (OperationCanceledException)
        {
            Console.WriteLine("The authentication process was aborted.");
        }

        catch (ProtocolException exception) when (exception.Error is Errors.AccessDenied)
        {
            Console.WriteLine("The authorization was denied by the end user.");
        }

        catch
        {
            Console.WriteLine("An error occurred while trying to authenticate the user.");
        }

        static async Task<string> GetResourceFromApi1Async(string token, CancellationToken cancellationToken)
        {
            using var client = new HttpClient();

            using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44342/api");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(request, cancellationToken);
            if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
            {
                return "The user represented by the access token is not allowed to access Api1.";
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        static async Task<string> GetResourceFromApi2Async(string token, CancellationToken cancellationToken)
        {
            using var client = new HttpClient();

            using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44379/api");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(request, cancellationToken);
            if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
            {
                return "The user represented by the access token is not allowed to access Api2.";
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

    }
}
