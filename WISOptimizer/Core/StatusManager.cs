using System;
using System.Threading.Tasks;

namespace WISOptimizer.Core
{
    public enum SettingStatus
    {
        Enabled,
        Disabled,
        Misconfigured,
        Unknown
    }

    public static class StatusManager
    {
        // Example method to detect a specific registry key
        public static async Task<SettingStatus> CheckRegistryValueAsync(string path, string name, string expectedValue)
        {
            var command = $"(Get-ItemProperty -Path '{path}' -Name '{name}' -ErrorAction SilentlyContinue).'{name}'";
            var result = await PowerShellRunner.RunCommandAsync(command);

            if (!result.Success || string.IsNullOrWhiteSpace(result.Output))
            {
                return SettingStatus.Disabled;
            }

            string actualValue = result.Output.Trim();
            if (actualValue == expectedValue)
            {
                return SettingStatus.Enabled;
            }
            else
            {
                return SettingStatus.Misconfigured;
            }
        }

        // Example method to detect a service state
        public static async Task<SettingStatus> CheckServiceStateAsync(string serviceName)
        {
            var command = $"(Get-Service -Name '{serviceName}' -ErrorAction SilentlyContinue).Status";
            var result = await PowerShellRunner.RunCommandAsync(command);

            if (!result.Success || string.IsNullOrWhiteSpace(result.Output))
            {
                return SettingStatus.Unknown;
            }

            string status = result.Output.Trim();
            if (status.Equals("Running", StringComparison.OrdinalIgnoreCase))
            {
                return SettingStatus.Enabled;
            }
            else if (status.Equals("Stopped", StringComparison.OrdinalIgnoreCase))
            {
                return SettingStatus.Disabled;
            }

            return SettingStatus.Misconfigured;
        }
    }
}
