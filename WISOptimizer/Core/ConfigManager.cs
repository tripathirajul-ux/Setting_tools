using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WISOptimizer.Core
{
    public class SettingsData
    {
        [JsonPropertyName("network")]
        public NetworkSettings Network { get; set; } = new NetworkSettings();

        [JsonPropertyName("camera")]
        public CameraSettings Camera { get; set; } = new CameraSettings();

        [JsonPropertyName("logging")]
        public LoggingSettings Logging { get; set; } = new LoggingSettings();

        [JsonPropertyName("storage")]
        public StorageSettings Storage { get; set; } = new StorageSettings();

        [JsonPropertyName("uninstall")]
        public UninstallSettings Uninstall { get; set; } = new UninstallSettings();

        [JsonIgnore] // Don't persist to JSON, load from system dynamically
        public OptimizationSettings Optimization { get; set; } = new OptimizationSettings();
    }

    public class NetworkSettings
    {
        [JsonPropertyName("ip")]
        public string Ip { get; set; } = "192.168.1.20";

        [JsonPropertyName("gateway")]
        public string Gateway { get; set; } = "192.168.1.1";

        [JsonPropertyName("dns")]
        public string Dns { get; set; } = "8.8.8.8";
    }

    public class CameraSettings
    {
        [JsonPropertyName("sdk_port")]
        public int SdkPort { get; set; } = 5000;
    }

    public class LoggingSettings
    {
        [JsonPropertyName("path")]
        public string Path { get; set; } = @"C:\WISOptimizer\Logs";
    }

    public class StorageSettings
    {
        [JsonPropertyName("min_free_space_gb")]
        public int MinFreeSpaceGb { get; set; } = 10;
    }

    public class UninstallSettings
    {
        [JsonPropertyName("one_drive_setup_path")]
        public string OneDriveSetupPath { get; set; } = "%SystemRoot%\\SysWOW64\\OneDriveSetup.exe";

        [JsonPropertyName("teams_machine_wide_installer_path")]
        public string TeamsMachineWideInstallerPath { get; set; } = "%ProgramFiles(x86)%\\Teams Installer\\Teams.exe";
    }

    public static class ConfigManager
    {
        private static readonly string ConfigPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "settings.json");

        // 🔒 Keep setter private (correct design)
        public static SettingsData CurrentSettings { get; private set; } = new SettingsData();

        // ✅ Load config from file
        public static void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    var data = JsonSerializer.Deserialize<SettingsData>(json);

                    if (data != null)
                        CurrentSettings = data;
                }
                else
                {
                    SaveConfig(); // create default file
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config: {ex.Message}");
            }
        }

        // ✅ Save config to file
        public static void SaveConfig()
        {
            try
            {
                string? dir = Path.GetDirectoryName(ConfigPath);

                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(CurrentSettings, options);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving config: {ex.Message}");
            }
        }

        // ✅ SAFE WAY to replace entire settings (fix for your error)
        public static void SetSettings(SettingsData newSettings)
        {
            if (newSettings == null)
                return;

            CurrentSettings = newSettings;
        }

        // ✅ SAFE WAY to update partial values
        public static void Update(Action<SettingsData> updateAction)
        {
            updateAction?.Invoke(CurrentSettings);
        }
    }
}