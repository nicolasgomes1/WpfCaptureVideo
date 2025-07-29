using System.Configuration;
using System.Data;
using Microsoft.Extensions.Hosting;

using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WpfRecorder.ViewModels;

namespace WpfRecorder;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static IHost? AppHost { get; set; }

    
    public App()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console() // Optional: for console output
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        AppHost = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices((services) =>
            {
                services.AddTransient<MainWindowViewModel>();
                services.AddSingleton<MainWindow>();
            }).Build();
        Log.Information("Application is starting.");

    }
    
    protected override async void OnExit(ExitEventArgs e)
    {
        if (AppHost is null) return;
        await AppHost.StopAsync();
        AppHost.Dispose();
        base.OnExit(e);
    }
}