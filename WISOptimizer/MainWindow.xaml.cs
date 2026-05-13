using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WISOptimizer.Core;

namespace WISOptimizer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeApp();
        }

        private void InitializeApp()
        {
            ConfigManager.LoadConfig();
            LoggingManager.LogInfo("Application Started | BUILD_MARKER=DBG_20260513_1332");
            // #region agent log
            DebugSessionLogger.Log(
                runId: "pre-fix",
                hypothesisId: "H0",
                location: "MainWindow.xaml.cs:20",
                message: "Application startup marker",
                data: new { baseDir = AppDomain.CurrentDomain.BaseDirectory, currentDir = Environment.CurrentDirectory });
            // #endregion
            StatusText.Text = "Ready — all systems operational [DBG_20260513_1332]";
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not string tag) return;

            StatusText.Text = $"Loading: {tag}...";

            MainContent.Content = tag switch
            {
                "SystemPerformance" => new UI.Views.SystemPerformanceView(),
                "PowerSleep"        => new UI.Views.PowerSleepView(),
                "Network"           => new UI.Views.NetworkView(),
                "Security"          => new UI.Views.SecurityView(),
                "Storage"           => new UI.Views.StorageView(),
                "Startup"           => new UI.Views.StartupServicesView(),
                "WindowsUpdate"     => new UI.Views.WindowsUpdateView(),
                "Logging"           => new UI.Views.LoggingMonitoringView(),
                "Recovery"          => new UI.Views.RecoveryWatchdogView(),
                "Taskbar"           => new UI.Views.TaskbarCustomizationView(),
                "Config"            => new UI.Views.ConfigurationPanel(),
                _                   => MainContent.Content
            };

            StatusText.Text = $"Viewing: {tag}";
        }

        private void ExportConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConfigManager.SaveConfig();

                MessageBox.Show(
                    "Configuration exported successfully.",
                    "Export Config",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoggingManager.LogInfo("Config exported by user.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Export failed: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RestoreDefaults_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Are you sure you want to restore default configuration values?",
                "Restore Defaults",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm == MessageBoxResult.Yes)
            {
                // ✅ FIXED: Use controlled setter (NOT direct assignment)
                ConfigManager.SetSettings(new SettingsData());

                ConfigManager.SaveConfig();

                MessageBox.Show(
                    "Defaults restored.",
                    "Restore Defaults",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoggingManager.LogInfo("Defaults restored by user.");
            }
        }

        private void ApplyAll_Click(object sender, RoutedEventArgs e)
        {
            if (MainContent.Content is not ICommandExportProvider provider)
            {
                MessageBox.Show("Open a settings panel first to apply selected optimizations.", "Apply All", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var commands = provider.GetSelectedPowerShellCommands();
            if (commands.Count == 0)
            {
                MessageBox.Show("No selected optimizations found in current panel.", "Apply All", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _ = ExecuteCommandsAsync(commands);
        }

        private async System.Threading.Tasks.Task ExecuteCommandsAsync(List<string> commands)
        {
            foreach (var command in commands)
            {
                await PowerShellRunner.RunCommandAsync(command);
            }
            MessageBox.Show("Selected optimizations applied.", "Apply All", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GenerateDeploymentScript_Click(object sender, RoutedEventArgs e)
        {
            if (MainContent.Content is not ICommandExportProvider provider)
            {
                MessageBox.Show("Open a settings panel first.", "Generate Deployment Script", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var commands = provider.GetSelectedPowerShellCommands();
            if (commands.Count == 0)
            {
                MessageBox.Show("No toggled optimizations found in current panel.", "Generate Deployment Script", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (DeploymentScriptManager.SavePowerShellScript(commands, out var savedPath))
            {
                MessageBox.Show($"Deployment script saved:\n{savedPath}", "Generate Deployment Script", MessageBoxButton.OK, MessageBoxImage.Information);
                LoggingManager.LogInfo($"Deployment script generated at {savedPath}");
            }
        }
    }
}