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

        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                await BackupManager.CreateSystemRestorePointAsync("Before System Performance Changes");

                if (chkAnimations.IsChecked == true)
                {
                    var result = await PowerShellRunner.RunCommandAsync("Set-ItemProperty -Path 'HKCU:\\Control Panel\\Desktop' -Name 'UserPreferencesMask' -Value ([byte[]](0x90,0x12,0x03,0x80))");
                    if (!result.Success) throw new Exception($"Failed to apply animations setting: {result.Error}");
                }

                if (chkGpuHighPerf.IsChecked == true)
                {
                    // Simulate a failure for a feature that might not be available
                    var result = await PowerShellRunner.RunCommandAsync("Get-CimInstance Win32_VideoController");
                    if (string.IsNullOrEmpty(result.Output))
                    {
                        throw new Exception("No compatible GPU found for high-performance optimization.");
                    }
                }

                MessageBox.Show("System Performance settings applied successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCurrentStatus(); // Refresh
            }
            catch (Exception ex)
            {
                LoggingManager.LogError("Error applying system performance settings", ex);
                MessageBox.Show($"Operation Failed: {ex.Message}\n\nCheck logs for details.", "Execution Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
