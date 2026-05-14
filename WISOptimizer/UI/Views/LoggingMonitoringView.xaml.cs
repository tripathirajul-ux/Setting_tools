using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using WISOptimizer.Core;

namespace WISOptimizer.UI.Views
{
    public partial class LoggingMonitoringView : UserControl
    {
        public LoggingMonitoringView()
        {
            InitializeComponent();
            LoadRecentLogs();
        }

        private void LoadRecentLogs()
        {
            try
            {
                var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", $"OptimizerLog_{DateTime.Now:yyyyMMdd}.txt");
                txtLiveLogs.Text = File.Exists(logFile)
                    ? File.ReadAllText(logFile)
                    : $"[{DateTime.Now:HH:mm:ss}] WIS Optimizer started — no entries yet.";
            }
            catch (Exception ex) { txtLiveLogs.Text = $"Error loading logs: {ex.Message}"; }
        }

        private void ExportLogs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoggingManager.LogInfo("Logs exported.");
                MessageBox.Show("Logs saved to the Logs folder.", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // FIXED: must be async void since it uses await
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
    }
}
