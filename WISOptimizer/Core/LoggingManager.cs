using System;
using System.IO;

namespace WISOptimizer.Core
{
    public static class LoggingManager
    {
        private static string GetLogPath()
        {
            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            return Path.Combine(logDir, $"OptimizerLog_{DateTime.Now:yyyyMMdd}.txt");
        }

        public static void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        public static void LogWarning(string message)
        {
            WriteLog("WARNING", message);
        }

        public static void LogError(string message, Exception? ex = null)
        {
            string fullMessage = message;
            if (ex != null)
            {
                fullMessage += $" | Exception: {ex.Message}";
            }
            WriteLog("ERROR", fullMessage);
        }

        private static void WriteLog(string level, string message)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}";
                File.AppendAllText(GetLogPath(), logEntry);
            }
            catch (Exception)
            {
                // Fallback or ignore if logging fails
            }
        }
    }
}
