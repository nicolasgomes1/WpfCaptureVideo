using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using Serilog;
using WpfRecorder.Services;

namespace WpfRecorder.Api;

public class ConnectToApiData
{
    private static readonly ILogger Logger = Log.ForContext(typeof(SettingsService));

    private static readonly string ConfigFilePath = "C:\\Users\\nicol\\RiderProjects\\WpfRecorder\\SaveDirectory.json";

    public static string LoadApiKeyFromJson()
    {
        try
        {
            if (!File.Exists(ConfigFilePath)) return null!;
            var json = File.ReadAllText(ConfigFilePath);
            using var doc = JsonDocument.Parse(json);
                
            if (doc.RootElement.TryGetProperty("ApiKey", out var ApiKey))
            {
                if (ApiKey.TryGetProperty("myApiKey", out var myApiKey))
                {
                    var api = myApiKey.GetString() ?? string.Empty;
                    Logger.Information("Loaded from JSON - ApiKey: {ApiKey}", api);
                    return api;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error loading settings from JSON");
        }
        return null!;
    }
    
    public static string LoadApiUriFromJson()
    {
        try
        {
            if (!File.Exists(ConfigFilePath)) return null!;
            var json = File.ReadAllText(ConfigFilePath);
            using var doc = JsonDocument.Parse(json);
                
            if (doc.RootElement.TryGetProperty("ApiKey", out var ApiKey))
            {
                if (ApiKey.TryGetProperty("myApiEndpoint", out var myApiEndpoint))
                {
                    var api = myApiEndpoint.GetString() ?? string.Empty;
                    Logger.Information("Loaded from JSON - ApiKey: {ApiKey}", api);
                    return api;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error loading settings from JSON");
        }
        return null!;
    }
    
    static void ConnectToApi()
    {
        string myApiEndpoint = LoadApiUriFromJson();

        var httpContext = new HttpClient();
        httpContext.BaseAddress = new Uri(myApiEndpoint);
        httpContext.DefaultRequestHeaders.Accept.Clear();
        httpContext.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        var response = httpContext.GetAsync("/ScreenCapture/keys").Result;
        var content = response.Content.ReadAsStringAsync().Result;
        Log.Information(content);
    }
    
    public static void ConnectToApiWithKey()
    {
        string myApikey = ConnectToApiData.LoadApiKeyFromJson();
        string myApiEndpoint = ConnectToApiData.LoadApiUriFromJson();
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(myApiEndpoint);


        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        try
        {
            var response = httpClient.GetAsync($"/ScreenCapture/{myApikey}/authorize").Result;

            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Error connecting to API: {StatusCode}", response.StatusCode);

                Application.Current.MainWindow.IsEnabled = false;
                
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

    
}