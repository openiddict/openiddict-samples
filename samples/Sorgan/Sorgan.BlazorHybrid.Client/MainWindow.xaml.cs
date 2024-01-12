using System;
using System.Windows;
using Dapplo.Microsoft.Extensions.Hosting.Wpf;

namespace Sorgan.BlazorHybrid.Client;

public partial class MainWindow : Window, IWpfShell
{
    public MainWindow(IServiceProvider provider)
    {
        InitializeComponent();

        Resources.Add("services", provider);
    }
}
