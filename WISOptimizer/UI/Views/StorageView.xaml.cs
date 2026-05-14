using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using WISOptimizer.Core;

namespace WISOptimizer.UI.Views
{
    public partial class StorageView : UserControl
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

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var script = WISOptimizer.Core.DeploymentScriptGenerator.GenerateMasterScript(WISOptimizer.Core.ConfigManager.CurrentSettings.Optimization);
                _ = WISOptimizer.Core.PowerShellRunner.RunCommandAsync(script, 120);
                MessageBox.Show("Optimizations applied.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public System.Collections.Generic.List<string> GetSelectedPowerShellCommands()
        {
            return new System.Collections.Generic.List<string>();
        }
    }
}
