using System;
using System.Windows;
using System.Windows.Controls;
using WISOptimizer.Core;

namespace WISOptimizer.UI.Views
{
    public partial class StartupServicesView : UserControl
    {
        public StartupServicesView()
        {
            InitializeComponent();
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

        private async void UninstallOneDrive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cmd = @"
                    Stop-Process -Name 'OneDrive' -Force -ErrorAction SilentlyContinue;
                    $setup1 = Join-Path $env:SystemRoot 'SysWOW64\OneDriveSetup.exe';
                    $setup2 = Join-Path $env:SystemRoot 'System32\OneDriveSetup.exe';
                    if (Test-Path $setup1) { Start-Process -FilePath $setup1 -ArgumentList '/uninstall' -Wait -NoNewWindow };
                    if (Test-Path $setup2) { Start-Process -FilePath $setup2 -ArgumentList '/uninstall' -Wait -NoNewWindow };
                    Remove-Item -Path ""$env:LOCALAPPDATA\Microsoft\OneDrive"" -Recurse -Force -ErrorAction SilentlyContinue;
                    Remove-Item -Path ""$env:ProgramData\Microsoft OneDrive"" -Recurse -Force -ErrorAction SilentlyContinue;
                    Remove-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'OneDrive' -ErrorAction SilentlyContinue";

                await PowerShellRunner.RunCommandAsync(cmd, 120);
                MessageBox.Show("OneDrive deep uninstall script completed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggingManager.LogError("OneDrive uninstall failed", ex);
                MessageBox.Show($"OneDrive uninstall failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void UninstallTeams_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cmd = @"
                    Stop-Process -Name 'Teams' -Force -ErrorAction SilentlyContinue;
                    Stop-Process -Name 'ms-teams' -Force -ErrorAction SilentlyContinue;
                    $machineWide = ""$env:ProgramFiles(x86)\Teams Installer\Teams.exe"";
                    if (Test-Path $machineWide) { Start-Process -FilePath $machineWide -ArgumentList '-uninstall -s' -Wait -NoNewWindow };
                    Get-AppxPackage -Name '*Teams*' -AllUsers | Remove-AppxPackage -AllUsers -ErrorAction SilentlyContinue;
                    Remove-Item -Path ""$env:LOCALAPPDATA\Microsoft\Teams"" -Recurse -Force -ErrorAction SilentlyContinue;
                    Remove-Item -Path ""$env:APPDATA\Microsoft\Teams"" -Recurse -Force -ErrorAction SilentlyContinue;
                    Remove-Item -Path ""$env:ProgramData\Teams"" -Recurse -Force -ErrorAction SilentlyContinue;
                    Remove-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'com.squirrel.Teams.Teams' -ErrorAction SilentlyContinue;
                    Remove-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'Teams' -ErrorAction SilentlyContinue";

                await PowerShellRunner.RunCommandAsync(cmd, 120);
                MessageBox.Show("Microsoft Teams deep uninstall script completed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggingManager.LogError("Teams uninstall failed", ex);
                MessageBox.Show($"Teams uninstall failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public System.Collections.Generic.List<string> GetSelectedPowerShellCommands()
        {
            return new System.Collections.Generic.List<string>();
        }
    }
}
