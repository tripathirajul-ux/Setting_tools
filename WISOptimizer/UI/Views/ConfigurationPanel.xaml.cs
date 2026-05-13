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
                if (string.IsNullOrWhiteSpace(txtIpAddress.Text)) throw new Exception("IP Address cannot be empty.");

                var cfg = ConfigManager.CurrentSettings;
                cfg.Network.Ip = txtIpAddress.Text;
                cfg.Network.Gateway = txtGateway.Text;
                cfg.Network.Dns = txtDns.Text;
                cfg.Logging.Path = txtLogPath.Text;

                if (int.TryParse(txtSdkPort.Text, out int port))
                    cfg.Camera.SdkPort = port;
                else
                    throw new Exception("SDK Port must be a valid number.");

                ConfigManager.SaveConfig();
                MessageBox.Show("Configuration saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggingManager.LogError("Error saving config", ex);
                MessageBox.Show($"Failed to save: {ex.Message}", "Config Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
