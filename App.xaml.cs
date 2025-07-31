using System.Configuration;
using System.Data;
using System.Net.Http;
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

    void ConnectToApi()
    {
        var httpContext = new HttpClient();
        httpContext.BaseAddress = new Uri("https://localhost:6969");
        httpContext.DefaultRequestHeaders.Accept.Clear();
        httpContext.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        var response = httpContext.GetAsync("/ScreenCapture/keys").Result;
        var content = response.Content.ReadAsStringAsync().Result;
        Log.Information(content);
    }

    void ConnectToApiWithKey()
    {
        const string mykey = "Nicolas-465465-00000-6989658";
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://localhost:6969");


        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            var response = httpClient.GetAsync($"/ScreenCapture/{mykey}/authorize").Result;

            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Error connecting to API: {StatusCode}", response.StatusCode);

                    Current.MainWindow.IsEnabled = false;
                
            }
            else
            {
                var content = response.Content.ReadAsStringAsync().Result;
                Log.Information("Authorization success: {Content}", content);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception during API call");
        }
    }

    
    

    
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
        ConnectToApiWithKey();
    }
    
    protected override async void OnExit(ExitEventArgs e)
    {
        if (AppHost is null) return;
        await AppHost.StopAsync();
        AppHost.Dispose();
        base.OnExit(e);
    }
}