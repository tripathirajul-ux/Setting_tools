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
