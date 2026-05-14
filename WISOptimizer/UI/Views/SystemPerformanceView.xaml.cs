using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WISOptimizer.Core;

namespace WISOptimizer.UI.Views
{
    public partial class SystemPerformanceView : UserControl
    {
        public SystemPerformanceView()
        {
            InitializeComponent();
            LoadCurrentStatus();
        }

        private async void LoadCurrentStatus()
        {
            try 
            {
                // Real detection logic would go here
                // For now, simulate detection
                UpdateStatusLabel(statusAnimations, true);
                UpdateStatusLabel(statusTransparency, false);
                UpdateStatusLabel(statusBestPerformance, true);
                UpdateStatusLabel(statusGpu, null); // Unknown/Misconfigured
                UpdateStatusLabel(statusBackground, true);
            }
            catch (Exception ex)
            {
                LoggingManager.LogError("Error detecting system status", ex);
                MessageBox.Show($"Failed to detect current status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateStatusLabel(TextBlock label, bool? isEnabled)
        {
            if (isEnabled == true)
            {
                label.Text = "✔ Enabled";
                label.Foreground = (SolidColorBrush)Application.Current.Resources["SuccessBrush"];
            }
            else if (isEnabled == false)
            {
                label.Text = "✖ Disabled";
                label.Foreground = (SolidColorBrush)Application.Current.Resources["DangerBrush"];
            }
            else
            {
                label.Text = "⚠ Misconfigured";
                label.Foreground = new SolidColorBrush(Color.FromRgb(226, 160, 63));
            }
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
    }
}
