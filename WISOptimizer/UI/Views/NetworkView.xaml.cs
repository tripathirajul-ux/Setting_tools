using System;
using System.Windows;
using System.Windows.Controls;
using WISOptimizer.Core;

namespace WISOptimizer.UI.Views
{
    public partial class NetworkView : UserControl
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

        public System.Collections.Generic.List<string> GetSelectedPowerShellCommands()
        {
            return new System.Collections.Generic.List<string>();
        }
    }
}
