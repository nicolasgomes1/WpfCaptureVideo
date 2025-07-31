using System.Configuration;
using System.Data;
using System.Net.Http;
using Microsoft.Extensions.Hosting;

using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WpfRecorder.Api;
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
        const string path = "C:\\Users\\nicol\\RiderProjects\\WpfRecorder\\Logs\\log-.txt";
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console() // Optional: for console output
            .WriteTo.File(path, rollingInterval: RollingInterval.Day)
            .CreateLogger();

        AppHost = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices((services) =>
            {
                services.AddTransient<MainWindowViewModel>();
            }).Build();
        Log.Information("Application is starting.");
      //  ConnectToApiWithKey();
    }
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var mainWindow = new MainWindow();
        MainWindow = mainWindow; // Set the MainWindow property
        mainWindow.Show();
        ConnectToApiData.ConnectToApiWithKey();
    }
    
    protected override async void OnExit(ExitEventArgs e)
    {
        if (AppHost is null) return;
        await AppHost.StopAsync();
        AppHost.Dispose();
        base.OnExit(e);
    }
}