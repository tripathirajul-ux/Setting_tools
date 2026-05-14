using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WISOptimizer.Core
{
    public class OptimizationSettings : INotifyPropertyChanged
    {
        // ═══ Windows Update ═══
        private bool _disableWindowsUpdate;
        public bool DisableWindowsUpdate { get => _disableWindowsUpdate; set => SetField(ref _disableWindowsUpdate, value); }

        private bool _disableDeliveryOpt;
        public bool DisableDeliveryOpt { get => _disableDeliveryOpt; set => SetField(ref _disableDeliveryOpt, value); }

        private bool _disableUpdateOrchestrator;
        public bool DisableUpdateOrchestrator { get => _disableUpdateOrchestrator; set => SetField(ref _disableUpdateOrchestrator, value); }

        // ═══ Taskbar Customization ═══
        private bool _taskbarLeft;
        public bool TaskbarLeft { get => _taskbarLeft; set => SetField(ref _taskbarLeft, value); }

        private bool _hideTaskView;
        public bool HideTaskView { get => _hideTaskView; set => SetField(ref _hideTaskView, value); }

        private bool _removeWidgets;
        public bool RemoveWidgets { get => _removeWidgets; set => SetField(ref _removeWidgets, value); }

        private bool _hideSearch;
        public bool HideSearch { get => _hideSearch; set => SetField(ref _hideSearch, value); }

        // ═══ System Performance ═══
        private bool _animations;
        public bool Animations { get => _animations; set => SetField(ref _animations, value); }

        private bool _transparency;
        public bool Transparency { get => _transparency; set => SetField(ref _transparency, value); }

        private bool _bestPerformance;
        public bool BestPerformance { get => _bestPerformance; set => SetField(ref _bestPerformance, value); }

        private bool _gpuHighPerf;
        public bool GpuHighPerf { get => _gpuHighPerf; set => SetField(ref _gpuHighPerf, value); }

        private bool _backgroundPriority;
        public bool BackgroundPriority { get => _backgroundPriority; set => SetField(ref _backgroundPriority, value); }

        // ═══ Storage ═══
        private bool _disableIndexing;
        public bool DisableIndexing { get => _disableIndexing; set => SetField(ref _disableIndexing, value); }

        private bool _disableDefrag;
        public bool DisableDefrag { get => _disableDefrag; set => SetField(ref _disableDefrag, value); }

        // ═══ Startup & Services ═══
        private bool _disableOneDrive;
        public bool DisableOneDrive { get => _disableOneDrive; set => SetField(ref _disableOneDrive, value); }

        private bool _disableTeams;
        public bool DisableTeams { get => _disableTeams; set => SetField(ref _disableTeams, value); }

        private bool _disableCopilot;
        public bool DisableCopilot { get => _disableCopilot; set => SetField(ref _disableCopilot, value); }

        private bool _disableWeather;
        public bool DisableWeather { get => _disableWeather; set => SetField(ref _disableWeather, value); }

        private bool _disableXbox;
        public bool DisableXbox { get => _disableXbox; set => SetField(ref _disableXbox, value); }

        private bool _disableBluetooth;
        public bool DisableBluetooth { get => _disableBluetooth; set => SetField(ref _disableBluetooth, value); }

        // ═══ Security ═══
        private bool _disableRealTime;
        public bool DisableRealTime { get => _disableRealTime; set => SetField(ref _disableRealTime, value); }

        private bool _firewall;
        public bool Firewall { get => _firewall; set => SetField(ref _firewall, value); }

        // ═══ Recovery Watchdog ═══
        private bool _disableAutoRestart;
        public bool DisableAutoRestart { get => _disableAutoRestart; set => SetField(ref _disableAutoRestart, value); }

        private bool _enableWatchdog;
        public bool EnableWatchdog { get => _enableWatchdog; set => SetField(ref _enableWatchdog, value); }

        private bool _restartFailedServices;
        public bool RestartFailedServices { get => _restartFailedServices; set => SetField(ref _restartFailedServices, value); }

        // ═══ Power & Sleep ═══
        private bool _highPerf;
        public bool HighPerf { get => _highPerf; set => SetField(ref _highPerf, value); }

        private bool _ultimatePerformance;
        public bool UltimatePerformance { get => _ultimatePerformance; set => SetField(ref _ultimatePerformance, value); }

        private bool _disableSleep;
        public bool DisableSleep { get => _disableSleep; set => SetField(ref _disableSleep, value); }

        private bool _disableHibernate;
        public bool DisableHibernate { get => _disableHibernate; set => SetField(ref _disableHibernate, value); }

        private bool _disableDisplay;
        public bool DisableDisplay { get => _disableDisplay; set => SetField(ref _disableDisplay, value); }

        // ═══ Network ═══
        private bool _staticIp;
        public bool StaticIp { get => _staticIp; set => SetField(ref _staticIp, value); }

        private bool _nicPowerSaving;
        public bool NicPowerSaving { get => _nicPowerSaving; set => SetField(ref _nicPowerSaving, value); }

        private bool _interrupt;
        public bool Interrupt { get => _interrupt; set => SetField(ref _interrupt, value); }

        // ═══ Logging & Monitoring ═══
        private bool _eventViewer;
        public bool EventViewer { get => _eventViewer; set => SetField(ref _eventViewer, value); }

        private bool _shutdownTracking;
        public bool ShutdownTracking { get => _shutdownTracking; set => SetField(ref _shutdownTracking, value); }

        private bool _crashTracking;
        public bool CrashTracking { get => _crashTracking; set => SetField(ref _crashTracking, value); }

        private bool _deviceDisconnect;
        public bool DeviceDisconnect { get => _deviceDisconnect; set => SetField(ref _deviceDisconnect, value); }

        // ═══ INotifyPropertyChanged ═══
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // Initialize with default state if desired, or leave false and load from system
        public OptimizationSettings()
        {
            // Initial defaults can be set here
            Firewall = true; // Example: Firewall usually defaults to true
        }

        public async System.Threading.Tasks.Task LoadCurrentSystemStateAsync()
        {
            string script = @"
$state = @{
    TaskbarLeft = $(try { (Get-ItemPropertyValue 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' 'TaskbarAl' -ErrorAction Stop) -eq 0 } catch { $false })
    HideTaskView = $(try { (Get-ItemPropertyValue 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' 'ShowTaskViewButton' -ErrorAction Stop) -eq 0 } catch { $false })
    RemoveWidgets = $(try { (Get-ItemPropertyValue 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' 'TaskbarDa' -ErrorAction Stop) -eq 0 } catch { $false })
    HideSearch = $(try { (Get-ItemPropertyValue 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Search' 'SearchboxTaskbarMode' -ErrorAction Stop) -eq 0 } catch { $false })
}
$state | ConvertTo-Json
";
            var result = await PowerShellRunner.RunCommandAsync(script);
            if (result.Success && !string.IsNullOrWhiteSpace(result.Output))
            {
                try
                {
                    // Find the JSON part in the output (PowerShell might output other things before it)
                    string json = result.Output.Trim();
                    int startIdx = json.IndexOf('{');
                    int endIdx = json.LastIndexOf('}');
                    if (startIdx >= 0 && endIdx > startIdx)
                    {
                        json = json.Substring(startIdx, endIdx - startIdx + 1);
                        var doc = System.Text.Json.JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("TaskbarLeft", out var tl)) TaskbarLeft = tl.GetBoolean();
                        if (root.TryGetProperty("HideTaskView", out var htv)) HideTaskView = htv.GetBoolean();
                        if (root.TryGetProperty("RemoveWidgets", out var rw)) RemoveWidgets = rw.GetBoolean();
                        if (root.TryGetProperty("HideSearch", out var hs)) HideSearch = hs.GetBoolean();
                    }
                }
                catch
                {
                    // Ignore parsing errors for now
                }
            }
        }
    }
}
