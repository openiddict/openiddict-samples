﻿using System;
using System.Threading;
using System.Windows;
using Dapplo.Microsoft.Extensions.Hosting.Wpf;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Abstractions.OpenIddictExceptions;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

namespace Sorgan.Wpf.Client;

public partial class MainWindow : Window, IWpfShell
{
    private readonly OpenIddictClientService _service;

    public MainWindow(OpenIddictClientService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));

        InitializeComponent();
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        // Disable the login button to prevent concurrent authentication operations.
        LoginButton.IsEnabled = false;

        try
        {
            using var source = new CancellationTokenSource(delay: TimeSpan.FromSeconds(90));

            try
            {
                // Ask OpenIddict to initiate the authentication flow (typically, by starting the system browser).
                var result = await _service.ChallengeInteractivelyAsync(new()
                {
                    CancellationToken = source.Token
                });

                // Wait for the user to complete the authorization process.
                var principal = (await _service.AuthenticateInteractivelyAsync(new()
                {
                    CancellationToken = source.Token,
                    Nonce = result.Nonce
                })).Principal;

                MessageBox.Show($"Welcome, {principal.FindFirst(Claims.Name)!.Value}.",
                    "Authentication successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            catch (OperationCanceledException)
            {
                MessageBox.Show("The authentication process was aborted.",
                    "Authentication timed out", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            catch (ProtocolException exception) when (exception.Error is Errors.AccessDenied)
            {
                MessageBox.Show("The authorization was denied by the end user.",
                    "Authorization denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            catch
            {
                MessageBox.Show("An error occurred while trying to authenticate the user.",
                    "Authentication failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        finally
        {
            // Re-enable the login button to allow starting a new authentication operation.
            LoginButton.IsEnabled = true;
        }
    }
}
