using System;
using System.Windows;
using System.Windows.Controls;
using WISOptimizer.Core;

namespace WISOptimizer.UI.Views
{
    public partial class RecoveryWatchdogView : UserControl
    {
        public RecoveryWatchdogView()
        {
            InitializeComponent();
        }

        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await BackupManager.CreateSystemRestorePointAsync("Before Recovery Changes");

                if (chkDisableAutoRestart.IsChecked == true)
                {
                    var result = await PowerShellRunner.RunCommandAsync(
                        "Set-ItemProperty -Path 'HKLM:\\System\\CurrentControlSet\\Control\\CrashControl' -Name 'AutoReboot' -Value 0");
                    if (!result.Success) throw new Exception($"Registry update failed: {result.Error}");
                }

                MessageBox.Show("Recovery & Watchdog settings applied.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggingManager.LogError("Error applying recovery settings", ex);
                MessageBox.Show($"Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
