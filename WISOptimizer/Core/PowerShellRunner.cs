using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WISOptimizer.Core
{
    public class ExecutionResult
    {
        public bool Success { get; set; } = false;
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }

    public static class PowerShellRunner
    {
        // Default timeout: 30 seconds per command
        public static async Task<ExecutionResult> RunCommandAsync(string command, int timeoutSeconds = 30)
        {
            return await Task.Run(() =>
            {
                var result = new ExecutionResult();
                try
                {
                    // #region agent log
                    DebugSessionLogger.Log(
                        runId: "pre-fix",
                        hypothesisId: "H6",
                        location: "PowerShellRunner.cs:26",
                        message: "PowerShell command entry",
                        data: new { timeoutSeconds, commandLength = command?.Length ?? 0 });
                    // #endregion

                    LoggingManager.LogInfo($"PS Execute: {command}");

                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"{command.Replace("\"", "\\\"")}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,   // Must be false for redirect
                        CreateNoWindow = true      // No visible window
                    };

                    using var process = new Process { StartInfo = processStartInfo };
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

                    process.Start();

                    // Read output asynchronously to prevent deadlocks
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    bool completed = process.WaitForExit(timeoutSeconds * 1000);

                    result.Output = outputTask.Result;
                    result.Error = errorTask.Result;

                    if (!completed)
                    {
                        try { process.Kill(); } catch { /* ignore */ }
                        result.Success = false;
                        result.Error = $"Command timed out after {timeoutSeconds} seconds.";
                        LoggingManager.LogWarning($"PS Timeout: {command}");
                        return result;
                    }

                    result.Success = process.ExitCode == 0;

                    if (result.Success)
                        LoggingManager.LogInfo($"PS OK (exit 0). Output: {result.Output?.Trim()}");
                    else
                        LoggingManager.LogWarning($"PS Failed (exit {process.ExitCode}). Error: {result.Error?.Trim()}");
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Error = ex.Message;
                    LoggingManager.LogError("PowerShellRunner Exception", ex);
                }

                return result;
            });
        }
    }
}
