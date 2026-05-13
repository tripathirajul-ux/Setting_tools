using System;
using System.IO;
using System.Threading.Tasks;

namespace WISOptimizer.Core
{
    public static class BackupManager
    {
        private static string GetBackupDir()
        {
            var backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup");
            if (!Directory.Exists(backupDir))
                Directory.CreateDirectory(backupDir);
            return backupDir;
        }

        public static async Task BackupRegistryKeyAsync(string registryPath, string filename)
        {
            string backupPath = Path.Combine(GetBackupDir(), $"{filename}_{DateTime.Now:yyyyMMdd_HHmmss}.reg");
            LoggingManager.LogInfo($"Backing up registry key '{registryPath}' to '{backupPath}'");
            await PowerShellRunner.RunCommandAsync($"reg export \"{registryPath}\" \"{backupPath}\" /y");
        }

        /// <summary>
        /// Creates a system restore point in a fire-and-forget manner so
        /// the UI is NEVER blocked waiting for it to complete.
        /// Windows also limits restore points to one per 24 hours, so failures are ignored.
        /// </summary>
        public static Task CreateSystemRestorePointAsync(string description)
        {
            LoggingManager.LogInfo($"Restore point requested: {description}");
            // Fire-and-forget — don't block the UI thread waiting for this
            Task.Run(async () =>
            {
                var result = await PowerShellRunner.RunCommandAsync(
                    $"Checkpoint-Computer -Description '{description}' -RestorePointType 'MODIFY_SETTINGS' -ErrorAction SilentlyContinue",
                    timeoutSeconds: 60);
                if (!result.Success)
                    LoggingManager.LogWarning($"Restore point skipped/failed (Windows may limit frequency): {result.Error}");
                else
                    LoggingManager.LogInfo($"Restore point created: {description}");
            });

            // Return a completed task immediately so callers don't block
            return Task.CompletedTask;
        }
    }
}
