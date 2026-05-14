using System;
using System.Windows;
using System.Windows.Controls;
using WISOptimizer.Core;

namespace WISOptimizer.UI.Views
{
    public partial class ConfigurationPanel : UserControl
    {
        public ConfigurationPanel()
        {
            InitializeComponent();
            LoadConfigData();
        }

        private void LoadConfigData()
        {
            try
            {
                var cfg = ConfigManager.CurrentSettings;
                txtIpAddress.Text = cfg.Network.Ip;
                txtGateway.Text = cfg.Network.Gateway;
                txtDns.Text = cfg.Network.Dns;
                txtSdkPort.Text = cfg.Camera.SdkPort.ToString();
                txtLogPath.Text = cfg.Logging.Path;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load config: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
