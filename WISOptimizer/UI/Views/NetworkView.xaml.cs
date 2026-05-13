using System;
using System.Windows;
using System.Windows.Controls;
using WISOptimizer.Core;

namespace WISOptimizer.UI.Views
{
    public partial class NetworkView : UserControl, ICommandExportProvider
    {
        public NetworkView()
        {
            InitializeComponent();
            LoadConfigData();
        }

        private void LoadConfigData()
        {
            try 
            {
                txtIpAddress.Text = ConfigManager.CurrentSettings.Network.Ip;
                txtGateway.Text = ConfigManager.CurrentSettings.Network.Gateway;
                txtDns.Text = ConfigManager.CurrentSettings.Network.Dns;
            }
            catch (Exception ex)
            {
                LoggingManager.LogError("Error loading network config", ex);
            }
        }

        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                // #region agent log
                DebugSessionLogger.Log(
                    runId: "pre-fix",
                    hypothesisId: "H5",
                    location: "NetworkView.xaml.cs:34",
                    message: "Network apply requested",
                    data: new
                    {
                        staticIp = chkStaticIp.IsChecked == true,
                        nicPowerSaving = chkNicPowerSaving.IsChecked == true,
                        interruptModeration = chkInterrupt.IsChecked == true,
                        ip = txtIpAddress.Text
                    });
                // #endregion

                await BackupManager.CreateSystemRestorePointAsync("Before Network Changes");

                if (chkStaticIp.IsChecked == true)
                {
                    if (string.IsNullOrWhiteSpace(txtIpAddress.Text)) throw new Exception("IP Address cannot be empty.");

                    ConfigManager.CurrentSettings.Network.Ip = txtIpAddress.Text;
                    ConfigManager.CurrentSettings.Network.Gateway = txtGateway.Text;
                    ConfigManager.CurrentSettings.Network.Dns = txtDns.Text;
                    ConfigManager.SaveConfig();
                    
                    var result = await PowerShellRunner.RunCommandAsync($"$adapter = Get-NetAdapter -Physical | Where-Object {{$_.Status -eq 'Up' -and $_.Name -notmatch 'Loopback|Virtual|vEthernet|VPN' -and $_.InterfaceDescription -notmatch 'Loopback|Virtual|Hyper-V|VPN'}} | Select-Object -First 1; if($adapter){{New-NetIPAddress -InterfaceAlias $adapter.Name -IPAddress {txtIpAddress.Text} -DefaultGateway {txtGateway.Text} -PrefixLength 24 -ErrorAction SilentlyContinue}}");
                    // #region agent log
                    DebugSessionLogger.Log(
                        runId: "pre-fix",
                        hypothesisId: "H5",
                        location: "NetworkView.xaml.cs:61",
                        message: "Static IP command result",
                        data: new { success = result.Success, error = result.Error, output = result.Output });
                    // #endregion
                    // We don't necessarily fail here because IP might already exist, but we log it
                }

                if (chkNicPowerSaving.IsChecked == true)
                {
                    await PowerShellRunner.RunCommandAsync("$adapter = Get-NetAdapter -Physical | Where-Object {$_.Status -eq 'Up' -and $_.Name -notmatch 'Loopback|Virtual|vEthernet|VPN' -and $_.InterfaceDescription -notmatch 'Loopback|Virtual|Hyper-V|VPN'} | Select-Object -First 1; if($adapter){Set-NetAdapterPowerManagement -Name $adapter.Name -AllowComputerToTurnOffDevice Disabled -NoRestart -ErrorAction SilentlyContinue}");
                }

                if (chkInterrupt.IsChecked == true)
                {
                    await PowerShellRunner.RunCommandAsync("$adapter = Get-NetAdapter -Physical | Where-Object {$_.Status -eq 'Up' -and $_.Name -notmatch 'Loopback|Virtual|vEthernet|VPN' -and $_.InterfaceDescription -notmatch 'Loopback|Virtual|Hyper-V|VPN'} | Select-Object -First 1; if($adapter){Set-NetAdapterAdvancedProperty -Name $adapter.Name -DisplayName 'Interrupt Moderation' -DisplayValue 'Disabled' -NoRestart -ErrorAction SilentlyContinue}");
                }

                MessageBox.Show("Network optimization settings applied.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggingManager.LogError("Error applying network settings", ex);
                MessageBox.Show($"Failed to apply network settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public System.Collections.Generic.List<string> GetSelectedPowerShellCommands()
        {
            var commands = new System.Collections.Generic.List<string>();
            var filter = "$adapter = Get-NetAdapter -Physical | Where-Object {$_.Status -eq 'Up' -and $_.Name -notmatch 'Loopback|Virtual|vEthernet|VPN' -and $_.InterfaceDescription -notmatch 'Loopback|Virtual|Hyper-V|VPN'} | Select-Object -First 1;";
            if (chkStaticIp.IsChecked == true)
                commands.Add($"{filter} if($adapter){{New-NetIPAddress -InterfaceAlias $adapter.Name -IPAddress {txtIpAddress.Text} -DefaultGateway {txtGateway.Text} -PrefixLength 24 -ErrorAction SilentlyContinue}}");
            if (chkNicPowerSaving.IsChecked == true)
                commands.Add($"{filter} if($adapter){{Set-NetAdapterPowerManagement -Name $adapter.Name -AllowComputerToTurnOffDevice Disabled -NoRestart -ErrorAction SilentlyContinue}}");
            if (chkInterrupt.IsChecked == true)
                commands.Add($"{filter} if($adapter){{Set-NetAdapterAdvancedProperty -Name $adapter.Name -DisplayName 'Interrupt Moderation' -DisplayValue 'Disabled' -NoRestart -ErrorAction SilentlyContinue}}");
            return commands;
        }
    }
}
