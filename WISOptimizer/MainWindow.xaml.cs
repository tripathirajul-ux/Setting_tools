using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WISOptimizer.Core;

namespace WISOptimizer
{
    public partial class MainWindow : Window
    {
        private bool _isDarkTheme = true;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = ConfigManager.CurrentSettings.Optimization;
            InitializeApp();
        }

        private void InitializeApp()
        {
            ConfigManager.LoadConfig();
            _ = ConfigManager.CurrentSettings.Optimization.LoadCurrentSystemStateAsync();
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

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            _isDarkTheme = !_isDarkTheme;
            string themePath = _isDarkTheme ? "UI/Themes/DarkTheme.xaml" : "UI/Themes/LightTheme.xaml";
            
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(themePath, UriKind.Relative) });
        }

        private void ApplyAll_Click(object sender, RoutedEventArgs e)
        {
            var script = DeploymentScriptGenerator.GenerateMasterScript(ConfigManager.CurrentSettings.Optimization);
            _ = ExecuteGlobalScriptAsync(script);
        }

        private async System.Threading.Tasks.Task ExecuteGlobalScriptAsync(string script)
        {
            // Execute the full script as one block
            var result = await PowerShellRunner.RunCommandAsync(script, 120);
            if (result.Success) {
                MessageBox.Show("All global optimizations applied successfully.", "Apply All", MessageBoxButton.OK, MessageBoxImage.Information);
            } else {
                MessageBox.Show($"Errors occurred during application:\n{result.Error}", "Apply All", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GenerateDeploymentScript_Click(object sender, RoutedEventArgs e)
        {
            var script = DeploymentScriptGenerator.GenerateMasterScript(ConfigManager.CurrentSettings.Optimization);

            if (DeploymentScriptManager.SavePowerShellScript(script, out var savedPath))
            {
                MessageBox.Show($"Master deployment script saved:\n{savedPath}", "Generate Deployment Script", MessageBoxButton.OK, MessageBoxImage.Information);
                LoggingManager.LogInfo($"Master deployment script generated at {savedPath}");
            }
        }
    }
}