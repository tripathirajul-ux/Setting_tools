using System;
using System.Windows;
using System.Windows.Controls;
using WISOptimizer.Core;

namespace WISOptimizer.UI.Views
{
    public partial class PowerSleepView : UserControl, ICommandExportProvider
    {
        public PowerSleepView()
        {
            InitializeComponent();
        }

        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                // #region agent log
                DebugSessionLogger.Log(
                    runId: "pre-fix",
                    hypothesisId: "H4",
                    location: "PowerSleepView.xaml.cs:20",
                    message: "Power apply requested",
                    data: new
                    {
                        disableHibernate = chkDisableHibernate.IsChecked == true,
                        disableSleep = chkDisableSleep.IsChecked == true
                    });
                // #endregion

                await BackupManager.CreateSystemRestorePointAsync("Before Power & Sleep Changes");

                if (chkUltimatePerformance.IsChecked == true)
                {
                    await PowerShellRunner.RunCommandAsync(
                        "$ultimate='e9a42b02-d5df-448d-aa00-03f14749eb61'; $existing=(powercfg -l | Select-String -Pattern $ultimate); if(-not $existing){powercfg -duplicatescheme $ultimate | Out-Null}; powercfg -setactive $ultimate");
                }

                if (chkDisableHibernate.IsChecked == true)
                {
                    var result = await PowerShellRunner.RunCommandAsync("powercfg -h off");
                    if (!result.Success) throw new Exception($"Failed to disable hibernate: {result.Error}");
                }

                if (chkDisableSleep.IsChecked == true)
                {
                    await PowerShellRunner.RunCommandAsync("powercfg -x -standby-timeout-ac 0");
                    await PowerShellRunner.RunCommandAsync("powercfg -x -standby-timeout-dc 0");
                }

                if (chkDisableDisplay.IsChecked == true)
                {
                    await PowerShellRunner.RunCommandAsync("powercfg -x -monitor-timeout-ac 0");
                    await PowerShellRunner.RunCommandAsync("powercfg -x -monitor-timeout-dc 0");
                }

                MessageBox.Show("Power & Sleep settings updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggingManager.LogError("Error updating power settings", ex);
                MessageBox.Show($"Failed to update power settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public System.Collections.Generic.List<string> GetSelectedPowerShellCommands()
        {
            var commands = new System.Collections.Generic.List<string>();
            if (chkUltimatePerformance.IsChecked == true)
                commands.Add("$ultimate='e9a42b02-d5df-448d-aa00-03f14749eb61'; $existing=(powercfg -l | Select-String -Pattern $ultimate); if(-not $existing){powercfg -duplicatescheme $ultimate | Out-Null}; powercfg -setactive $ultimate");
            if (chkDisableHibernate.IsChecked == true)
                commands.Add("powercfg -h off");
            if (chkDisableSleep.IsChecked == true)
            {
                commands.Add("powercfg -x -standby-timeout-ac 0");
                commands.Add("powercfg -x -standby-timeout-dc 0");
            }
            if (chkDisableDisplay.IsChecked == true)
            {
                commands.Add("powercfg -x -monitor-timeout-ac 0");
                commands.Add("powercfg -x -monitor-timeout-dc 0");
            }
            return commands;
        }
    }
}
