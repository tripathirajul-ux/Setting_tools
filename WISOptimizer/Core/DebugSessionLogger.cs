using System;
using System.IO;
using System.Text.Json;

namespace WISOptimizer.Core
{
    public static class DebugSessionLogger
    {
        private static readonly object Sync = new();
        private const string LogPath = @"c:\Users\RajulTripathiLM\OneDrive - Lohia Mechatronik\Desktop\Projects\Settings_tool\debug-85ec8b.log";
        private static readonly string FallbackPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "debug-85ec8b-fallback.log");

        public static void Log(string runId, string hypothesisId, string location, string message, object data)
        {
            try
            {
                var payload = new
                {
                    sessionId = "85ec8b",
                    runId,
                    hypothesisId,
                    location,
                    message,
                    data,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                var json = JsonSerializer.Serialize(payload);
                lock (Sync)
                {
                    var dir = Path.GetDirectoryName(LogPath);
                    if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    File.AppendAllText(LogPath, json + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var fallbackDir = Path.GetDirectoryName(FallbackPath);
                    if (!string.IsNullOrWhiteSpace(fallbackDir) && !Directory.Exists(fallbackDir))
                    {
                        Directory.CreateDirectory(fallbackDir);
                    }
                    File.AppendAllText(FallbackPath, $"{DateTime.Now:O} | failed primary debug write | {ex.Message}{Environment.NewLine}");
                    LoggingManager.LogWarning($"DebugSessionLogger primary write failed: {ex.Message}");
                }
                catch
                {
                    // Debug logging must never break optimizer flow.
                }
            }
        }
    }
}
