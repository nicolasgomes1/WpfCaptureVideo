using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Serilog;

namespace WpfRecorder.Services;

public static class SettingsService
{
    private static readonly ILogger Logger = Log.ForContext(typeof(SettingsService));

    
    private static readonly string ConfigFilePath = "C:\\Users\\nicol\\RiderProjects\\WpfRecorder\\SaveDirectory.json";

    public static void UpdateJsonKey(string parentKey, string key, string value)
    {
        try
        {
            JsonObject jsonObject;
        
            if (File.Exists(ConfigFilePath))
            {
                var existingJson = File.ReadAllText(ConfigFilePath);
                jsonObject = JsonNode.Parse(existingJson)?.AsObject() ?? new JsonObject();
            }
            else
            {
                jsonObject = new JsonObject();
            }
        
            // Ensure parent key exists
            if (!jsonObject.ContainsKey(parentKey))
            {
                jsonObject[parentKey] = new JsonObject();
            }
        
            // Update specific key
            jsonObject[parentKey]![key] = value;
        
            // Save back
            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(jsonObject, options);
            File.WriteAllText(ConfigFilePath, jsonString);
        
            Logger.Information("Updated JSON key {ParentKey}.{Key}: {Value}", parentKey, key, value);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error updating JSON key {Key}", key);
        }
    }

}