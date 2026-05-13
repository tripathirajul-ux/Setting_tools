using System;
using System.Windows;
using System.Windows.Controls;
using WISOptimizer.Core;

namespace WISOptimizer.UI.Views
{
    public partial class TaskbarCustomizationView : UserControl, ICommandExportProvider
    {
        public TaskbarCustomizationView()
        {
            InitializeComponent();
        }

        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // #region agent log
                DebugSessionLogger.Log(
                    runId: "pre-fix",
                    hypothesisId: "H3",
                    location: "TaskbarCustomizationView.xaml.cs:20",
                    message: "Taskbar apply requested",
                    data: new
                    {
                        taskbarLeft = chkTaskbarLeft.IsChecked == true,
                        hideTaskView = chkHideTaskView.IsChecked == true,
                        removeWidgets = chkRemoveWidgets.IsChecked == true
                    });
                // #endregion

                await BackupManager.CreateSystemRestorePointAsync("Before UI Customization");

                if (chkTaskbarLeft.IsChecked == true)
                {
                    var leftResult = await PowerShellRunner.RunCommandAsync(
                        "Set-ItemProperty -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced' -Name 'TaskbarAl' -Value 0");
                    // #region agent log
                    DebugSessionLogger.Log(
                        runId: "pre-fix",
                        hypothesisId: "H3",
                        location: "TaskbarCustomizationView.xaml.cs:36",
                        message: "Taskbar left command result",
                        data: new { success = leftResult.Success, error = leftResult.Error });
                    // #endregion
                }

                if (chkHideTaskView.IsChecked == true)
                {
                    var taskViewResult = await PowerShellRunner.RunCommandAsync(
                        "Set-ItemProperty -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced' -Name 'ShowTaskViewButton' -Value 0");
                    // #region agent log
                    DebugSessionLogger.Log(
                        runId: "pre-fix",
                        hypothesisId: "H3",
                        location: "TaskbarCustomizationView.xaml.cs:49",
                        message: "Task view command result",
                        data: new { success = taskViewResult.Success, error = taskViewResult.Error });
                    // #endregion
                }

                if (chkRemoveWidgets.IsChecked == true)
                {
                    var widgetResult = await PowerShellRunner.RunCommandAsync(
                        "New-Item -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced' -Force | Out-Null; New-ItemProperty -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced' -Name 'TaskbarDa' -PropertyType DWord -Value 0 -Force");
                    // #region agent log
                    DebugSessionLogger.Log(
                        runId: "pre-fix",
                        hypothesisId: "H3",
                        location: "TaskbarCustomizationView.xaml.cs:62",
                        message: "Widgets command result",
                        data: new { success = widgetResult.Success, error = widgetResult.Error });
                    // #endregion
                }

                if (chkHideSearch.IsChecked == true)
                {
                    await PowerShellRunner.RunCommandAsync(
                        "New-Item -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Search' -Force | Out-Null; New-ItemProperty -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Search' -Name 'SearchboxTaskbarMode' -PropertyType DWord -Value 0 -Force");
                }

                // Restart Explorer to apply changes
                var restartResult = await PowerShellRunner.RunCommandAsync("Stop-Process -Name Explorer -Force");
                // #region agent log
                DebugSessionLogger.Log(
                    runId: "pre-fix",
                    hypothesisId: "H3",
                    location: "TaskbarCustomizationView.xaml.cs:74",
                    message: "Explorer restart result",
                    data: new { success = restartResult.Success, error = restartResult.Error });
                // #endregion

                MessageBox.Show("Taskbar settings applied. Explorer restarted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                LoggingManager.LogError("Error applying taskbar settings", ex);
                MessageBox.Show($"Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public System.Collections.Generic.List<string> GetSelectedPowerShellCommands()
        {
            var commands = new System.Collections.Generic.List<string>();
            if (chkTaskbarLeft.IsChecked == true)
                commands.Add("Set-ItemProperty -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced' -Name 'TaskbarAl' -Value 0");
            if (chkHideTaskView.IsChecked == true)
                commands.Add("Set-ItemProperty -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced' -Name 'ShowTaskViewButton' -Value 0");
            if (chkRemoveWidgets.IsChecked == true)
                commands.Add("New-Item -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced' -Force | Out-Null; New-ItemProperty -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced' -Name 'TaskbarDa' -PropertyType DWord -Value 0 -Force");
            if (chkHideSearch.IsChecked == true)
                commands.Add("New-Item -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Search' -Force | Out-Null; New-ItemProperty -Path 'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Search' -Name 'SearchboxTaskbarMode' -PropertyType DWord -Value 0 -Force");
            if (commands.Count > 0)
                commands.Add("Stop-Process -Name Explorer -Force");
            return commands;
        }
    }
}
