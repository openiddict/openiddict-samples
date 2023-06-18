using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OpenIddict.Client;
using Spectre.Console;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Abstractions.OpenIddictExceptions;

namespace Matty.Client;

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

        try
        {
            // Ask OpenIddict to send a device authorization request and write
            // the complete verification endpoint URI to the console output.
            var result = await _service.ChallengeUsingDeviceAsync(new()
            {
                CancellationToken = stoppingToken
            });

            if (result.VerificationUriComplete is not null)
            {
                AnsiConsole.MarkupLineInterpolated(
                    $"[yellow]Please visit [link]{result.VerificationUriComplete}[/] and confirm the displayed code is '{result.UserCode}' to complete the authentication demand.[/]");
            }

            else
            {
                AnsiConsole.MarkupLineInterpolated(
                    $"[yellow]Please visit [link]{result.VerificationUri}[/] and enter '{result.UserCode}' to complete the authentication demand.[/]");
            }

            // Wait for the user to complete the demand on the other device.
            var principal = (await _service.AuthenticateWithDeviceAsync(new()
            {
                DeviceCode = result.DeviceCode,
                Interval = result.Interval,
                Timeout = result.ExpiresIn < TimeSpan.FromMinutes(5) ? result.ExpiresIn : TimeSpan.FromMinutes(5)
            })).Principal;

            AnsiConsole.MarkupLine("[green]Authentication successful:[/]");

            var table = new Table()
                .AddColumn(new TableColumn("Claim type").Centered())
                .AddColumn(new TableColumn("Claim value type").Centered())
                .AddColumn(new TableColumn("Claim value").Centered());

            foreach (var claim in principal.Claims)
            {
                table.AddRow(
                    claim.Type.EscapeMarkup(),
                    claim.ValueType.EscapeMarkup(),
                    claim.Value.EscapeMarkup());
            }

            AnsiConsole.Write(table);
        }

        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("[red]The authentication process was aborted.[/]");
        }

        catch (ProtocolException exception) when (exception.Error is Errors.AccessDenied)
        {
            AnsiConsole.MarkupLine("[yellow]The authorization was denied by the end user.[/]");
        }

        catch
        {
            AnsiConsole.MarkupLine("[red]An error occurred while trying to authenticate the user.[/]");
        }
    }
}
