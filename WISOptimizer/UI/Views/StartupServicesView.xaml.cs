using System;
using System.Windows;
using System.Windows.Controls;
using WISOptimizer.Core;

namespace WISOptimizer.UI.Views
{
    public partial class StartupServicesView : UserControl, ICommandExportProvider
    {
        public StartupServicesView()
        {
            InitializeComponent();
        }

        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await BackupManager.CreateSystemRestorePointAsync("Before Startup Cleanup");

                // 1. Xbox Services
                if (chkDisableXbox.IsChecked == true)
                {
                    await PowerShellRunner.RunCommandAsync(
                        "Get-Service -Name XblAuthManager, XblGameSave, XboxNetApiSvc, XboxGipSvc -ErrorAction SilentlyContinue | Set-Service -StartupType Disabled");
                }

                // 2. Bluetooth Services
                if (chkDisableBluetooth.IsChecked == true)
                {
                    await PowerShellRunner.RunCommandAsync(
                        "Get-Service bthserv | Set-Service -StartupType Disabled -PassThru | Stop-Service -Force -ErrorAction SilentlyContinue");
                }

                // 3. OneDrive (Process + Registry AutoRun)
                if (chkDisableOneDrive.IsChecked == true)
                {
                    string oneDriveCmd = @"
                        Stop-Process -Name 'OneDrive' -Force -ErrorAction SilentlyContinue;
                        Remove-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'OneDrive' -ErrorAction SilentlyContinue";
                    await PowerShellRunner.RunCommandAsync(oneDriveCmd);
                }

                // 4. Microsoft Teams (Process + Registry AutoRun)
                if (chkDisableTeams.IsChecked == true)
                {
                    string teamsCmd = @"
                        Stop-Process -Name 'Teams' -Force -ErrorAction SilentlyContinue;
                        Remove-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'com.squirrel.Teams.Teams' -ErrorAction SilentlyContinue;
                        Remove-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'Teams' -ErrorAction SilentlyContinue";
                    await PowerShellRunner.RunCommandAsync(teamsCmd);
                }

                // 5. Copilot (Taskbar Icon)
                if (chkDisableCopilot.IsChecked == true)
                {
                    string copilotCmd = @"
                        $path = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced';
                        if (!(Test-Path $path)) { New-Item $path -Force };
                        Set-ItemProperty -Path $path -Name 'ShowCopilotButton' -Value 0";
                    await PowerShellRunner.RunCommandAsync(copilotCmd);
                }

                // 6. Weather & Widgets
                // Change "chkDisableWidgets" to "chkDisableWeather" to match your XAML
                if (chkDisableWeather.IsChecked == true)
                {
                    string widgetCmd = @"
                        $path = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced';
                        if (!(Test-Path $path)) { New-Item $path -Force };
                        Set-ItemProperty -Path $path -Name 'TaskbarDa' -Value 0"; // Windows 11 Widgets
                    
                    await PowerShellRunner.RunCommandAsync(widgetCmd);
                }

                MessageBox.Show("Selected optimizations applied. A restart may be required for some changes.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggingManager.LogError("Error during startup cleanup", ex);
                MessageBox.Show($"Cleanup failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            var commands = new System.Collections.Generic.List<string>();
            if (chkDisableXbox.IsChecked == true)
                commands.Add("Get-Service -Name XblAuthManager, XblGameSave, XboxNetApiSvc, XboxGipSvc -ErrorAction SilentlyContinue | Set-Service -StartupType Disabled");
            if (chkDisableBluetooth.IsChecked == true)
                commands.Add("Get-Service bthserv | Set-Service -StartupType Disabled -PassThru | Stop-Service -Force -ErrorAction SilentlyContinue");
            if (chkDisableOneDrive.IsChecked == true)
                commands.Add("Stop-Process -Name 'OneDrive' -Force -ErrorAction SilentlyContinue; Remove-ItemProperty -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Run' -Name 'OneDrive' -ErrorAction SilentlyContinue");
            if (chkDisableTeams.IsChecked == true)
                commands.Add("Stop-Process -Name 'Teams' -Force -ErrorAction SilentlyContinue; Remove-ItemProperty -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Run' -Name 'com.squirrel.Teams.Teams' -ErrorAction SilentlyContinue; Remove-ItemProperty -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Run' -Name 'Teams' -ErrorAction SilentlyContinue");
            if (chkDisableCopilot.IsChecked == true)
                commands.Add("$path='HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced'; if (!(Test-Path $path)) { New-Item $path -Force }; Set-ItemProperty -Path $path -Name 'ShowCopilotButton' -Value 0");
            if (chkDisableWeather.IsChecked == true)
                commands.Add("$path='HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced'; if (!(Test-Path $path)) { New-Item $path -Force }; Set-ItemProperty -Path $path -Name 'TaskbarDa' -Value 0");
            return commands;
        }
    }
}
