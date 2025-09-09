using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Serilog;

namespace WpfRecorder.Services;

public static class SettingsService
{
    private static readonly ILogger Logger = Log.ForContext(typeof(SettingsService));


 private static readonly string ConfigFilePath =
     Path.Combine(AppContext.BaseDirectory, "SaveDirectory.json");
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

    public static void LoadFromJson(ref string videoPath, ref string picturePath)
    {
        try
        {
            if (!File.Exists(ConfigFilePath)) return;
            var json = File.ReadAllText(ConfigFilePath);
            using var doc = JsonDocument.Parse(json);

            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if(!Directory.Exists($"{userProfile}\\QAFlow"))
                Directory.CreateDirectory($"{userProfile}\\QAFlow");
            
            if(!Directory.Exists($"{userProfile}\\QAFlow\\Videos"))
                Directory.CreateDirectory($"{userProfile}\\QAFlow\\Videos");
            
            if(!Directory.Exists($"{userProfile}\\QAFlow\\Screenshoots"))
                Directory.CreateDirectory($"{userProfile}\\QAFlow\\Screenshoots");
            
            if(!Directory.Exists($"{userProfile}\\QAFlow\\Logs"))
                Directory.CreateDirectory($"{userProfile}\\QAFlow\\Logs");
            
            
            if (doc.RootElement.TryGetProperty("SaveDirectories", out var saveDirectories))
            {
                if (saveDirectories.TryGetProperty("VideoDir", out var videoDir))
                {
                    // Replace the hardcoded "C:\\Users\\nicol" with the dynamic user profile path
                    videoPath = videoDir.GetString()?.Replace("C:\\Users\\nicol", userProfile) ?? string.Empty;
                }

                if (saveDirectories.TryGetProperty("PictureDir", out var pictureDir))
                {
                    // Replace the hardcoded "C:\\Users\\nicol" with the dynamic user profile path
                    picturePath = pictureDir.GetString()?.Replace("C:\\Users\\nicol", userProfile) ?? string.Empty;
                }
            }

            Logger.Information("Loaded from JSON - VideoDir: {VideoPath}, PictureDir: {PicturePath}", 
                videoPath, picturePath);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error loading settings from JSON");
        }
    }

}