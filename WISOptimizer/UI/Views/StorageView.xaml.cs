using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using WISOptimizer.Core;

namespace WISOptimizer.UI.Views
{
    public partial class StorageView : UserControl, ICommandExportProvider
    {
        public StorageView()
        {
            InitializeComponent();
            LoadCurrentStatus();
            // #region agent log
            DebugSessionLogger.Log(
                runId: "pre-fix",
                hypothesisId: "H1",
                location: "StorageView.xaml.cs:13",
                message: "Storage view initialized",
                data: new { statusText = txtStatus.Text, minFreeSpaceText = txtMinFreeSpace.Text });
            // #endregion
        }

        private void LoadCurrentStatus()
        {
            try
            {
                txtMinFreeSpace.Text = ConfigManager.CurrentSettings.Storage.MinFreeSpaceGb.ToString();
                RefreshDriveStatus();
            }
            catch (Exception ex)
            {
                LoggingManager.LogError("Failed to load storage status", ex);
            }
        }

        private void RefreshDriveStatus()
        {
            var drive = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\");
            var freeGb = drive.AvailableFreeSpace / (1024d * 1024d * 1024d);
            var threshold = ConfigManager.CurrentSettings.Storage.MinFreeSpaceGb;
            var isHealthy = freeGb >= threshold;
            txtStatus.Text = $"C: Drive - {freeGb:F1} GB free (Min {threshold} GB)";
            txtStatus.Foreground = (System.Windows.Media.SolidColorBrush)Application.Current.Resources[isHealthy ? "SuccessBrush" : "DangerBrush"];
        }

        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                // #region agent log
                DebugSessionLogger.Log(
                    runId: "pre-fix",
                    hypothesisId: "H2",
                    location: "StorageView.xaml.cs:24",
                    message: "Storage apply requested",
                    data: new
                    {
                        disableIndexing = chkDisableIndexing.IsChecked == true,
                        disableDefrag = chkDisableDefrag.IsChecked == true,
                        minFreeSpaceText = txtMinFreeSpace.Text
                    });
                // #endregion

                await BackupManager.CreateSystemRestorePointAsync("Before Storage Changes");

                if (chkDisableIndexing.IsChecked == true)
                {
                    var result = await PowerShellRunner.RunCommandAsync("Stop-Service WSearch -Force; Set-Service WSearch -StartupType Disabled");
                    if (!result.Success) throw new Exception($"Failed to disable indexing: {result.Error}");
                }

                if (chkDisableDefrag.IsChecked == true)
                {
                    await PowerShellRunner.RunCommandAsync("Disable-ScheduledTask -TaskPath '\\Microsoft\\Windows\\Defrag\\' -TaskName 'ScheduledDefrag' -ErrorAction SilentlyContinue");
                }

                if (int.TryParse(txtMinFreeSpace.Text, out var minFreeSpace))
                {
                    ConfigManager.CurrentSettings.Storage.MinFreeSpaceGb = minFreeSpace;
                    ConfigManager.SaveConfig();
                }
                RefreshDriveStatus();

                MessageBox.Show("Storage settings applied successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggingManager.LogError("Error updating storage settings", ex);
                MessageBox.Show($"Failed to update storage settings: {ex.Message}", "Storage Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public System.Collections.Generic.List<string> GetSelectedPowerShellCommands()
        {
            var commands = new System.Collections.Generic.List<string>();
            if (chkDisableIndexing.IsChecked == true)
                commands.Add("Stop-Service WSearch -Force; Set-Service WSearch -StartupType Disabled");
            if (chkDisableDefrag.IsChecked == true)
                commands.Add("Disable-ScheduledTask -TaskPath '\\Microsoft\\Windows\\Defrag\\' -TaskName 'ScheduledDefrag' -ErrorAction SilentlyContinue");
            return commands;
        }
    }
}
