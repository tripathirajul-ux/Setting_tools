using System;
using System.Windows;
using System.Windows.Controls;
using WISOptimizer.Core;

namespace WISOptimizer.UI.Views
{
    public partial class SecurityView : UserControl
    {
        public SecurityView()
        {
            InitializeComponent();
        }

        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                await BackupManager.CreateSystemRestorePointAsync("Before Security Changes");

                if (chkDisableRealTime.IsChecked == true)
                {
                    var result = await PowerShellRunner.RunCommandAsync("Set-MpPreference -DisableRealtimeMonitoring $true");
                    if (!result.Success) throw new Exception($"Failed to disable Defender: {result.Error}");
                }

                if (!string.IsNullOrWhiteSpace(txtExclusions.Text))
                {
                    var paths = txtExclusions.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var path in paths)
                    {
                        await PowerShellRunner.RunCommandAsync($"Add-MpPreference -ExclusionPath '{path.Trim()}'");
                    }
                }

                MessageBox.Show("Security settings applied.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggingManager.LogError("Error updating security settings", ex);
                MessageBox.Show($"Failed to update security settings: {ex.Message}", "Security Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
