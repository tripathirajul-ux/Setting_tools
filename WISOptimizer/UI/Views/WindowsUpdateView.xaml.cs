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

        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await BackupManager.CreateSystemRestorePointAsync("Before Windows Update Changes");

                if (chkDisableWindowsUpdate.IsChecked == true)
                {
                    var result = await PowerShellRunner.RunCommandAsync(
                        "Stop-Service wuauserv -Force -ErrorAction SilentlyContinue; Set-Service wuauserv -StartupType Disabled");
                    if (!result.Success) throw new Exception($"Failed to disable Windows Update: {result.Error}");
                    lblWuStatus.Text = "● Stopped (OK)";
                    lblWuStatus.Foreground = (SolidColorBrush)Application.Current.Resources["SuccessBrush"];
                }

                if (chkDisableDeliveryOpt.IsChecked == true)
                {
                    await PowerShellRunner.RunCommandAsync(
                        "Stop-Service DoSvc -Force -ErrorAction SilentlyContinue; Set-Service DoSvc -StartupType Disabled");
                    lblDoStatus.Text = "● Stopped (OK)";
                    lblDoStatus.Foreground = (SolidColorBrush)Application.Current.Resources["SuccessBrush"];
                }

                if (chkDisableUpdateOrchestrator.IsChecked == true)
                {
                    await PowerShellRunner.RunCommandAsync(
                        "Stop-Service UsoSvc -Force -ErrorAction SilentlyContinue; Set-Service UsoSvc -StartupType Disabled");
                }

                MessageBox.Show("Windows Update settings applied.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggingManager.LogError("Error disabling Windows Update services", ex);
                MessageBox.Show($"Operation failed: {ex.Message}", "Update Control Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
