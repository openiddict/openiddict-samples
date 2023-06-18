using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OpenIddict.Client;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Abstractions.OpenIddictExceptions;

namespace Mimban.Client;

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
                CancellationToken = stoppingToken
            });

            Console.WriteLine("System browser launched.");

            // Wait for the user to complete the authorization process.
            var response = await _service.AuthenticateInteractivelyAsync(new()
            {
                Nonce = result.Nonce
            });

            Console.WriteLine("Your GitHub identifier is: {0}", await GetResourceAsync(
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

        static async Task<string> GetResourceAsync(string token, CancellationToken cancellationToken)
        {
            using var client = new HttpClient();

            using var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:44383/api");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
    }
}
