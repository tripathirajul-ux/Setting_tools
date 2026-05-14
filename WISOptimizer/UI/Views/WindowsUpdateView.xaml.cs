using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WISOptimizer.Core;

namespace WISOptimizer.UI.Views
{
    public partial class WindowsUpdateView : UserControl
    {
        public WindowsUpdateView()
        {
            InitializeComponent();
            LoadCurrentStatus();
        }

        private async void LoadCurrentStatus()
        {
            try
            {
                var wuStatus = await StatusManager.CheckServiceStateAsync("wuauserv");
                UpdateServiceLabel(lblWuStatus, wuStatus);

                var doStatus = await StatusManager.CheckServiceStateAsync("DoSvc");
                UpdateServiceLabel(lblDoStatus, doStatus);
            }
            catch (Exception ex)
            {
                LoggingManager.LogError("Error detecting update service status", ex);
            }
        }

        private void UpdateServiceLabel(TextBlock label, SettingStatus status)
        {
            if (status == SettingStatus.Enabled)
            {
                label.Text = "● Running";
                label.Foreground = new SolidColorBrush(Colors.Orange);
            }
            else if (status == SettingStatus.Disabled)
            {
                label.Text = "● Stopped (OK)";
                label.Foreground = (SolidColorBrush)Application.Current.Resources["SuccessBrush"];
            }
            else
            {
                label.Text = "● Unknown";
                label.Foreground = (SolidColorBrush)Application.Current.Resources["SecondaryTextBrush"];
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
