using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using FlexSliceCompanion.Core;
using FlexSliceCompanion.Core.Apps;
using FlexSliceCompanion.Core.Audio;
using FlexSliceCompanion.Core.Config;
using FlexSliceCompanion.Core.Radio;
using FlexSliceCompanion.Core.Slices;
using FlexSliceCompanion.Plugins.Wsjtx;
using FlexSliceCompanion.Windows.Services;

namespace FlexSliceCompanion.Windows.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly IRadioClient _radioClient;
    private readonly AudioDeviceScanner _audioDeviceScanner;
    private readonly AppConfig _config;
    private readonly WsjtxConfigManager _wsjtxConfigManager;
    private readonly WsjtxInstallLocator _wsjtxInstallLocator;
    private readonly DaxEndpointDetector _daxDetector = new();
    private SliceSessionManager? _sessionManager;
    private DaxScanResult? _latestDaxScan;
    private RadioInfo? _selectedRadio;
    private SliceState? _selectedSlice;
    private string _status = "Ready";
    private string _daxMessage = string.Empty;

    public MainWindowViewModel(
        IRadioClient radioClient,
        AudioDeviceScanner audioDeviceScanner,
        AppConfig config,
        WsjtxConfigManager wsjtxConfigManager,
        WsjtxInstallLocator wsjtxInstallLocator)
    {
        _radioClient = radioClient;
        _audioDeviceScanner = audioDeviceScanner;
        _config = config;
        _wsjtxConfigManager = wsjtxConfigManager;
        _wsjtxInstallLocator = wsjtxInstallLocator;

        _radioClient.SliceChanged.Subscribe(new ActionObserver<SliceState>(OnSliceChanged));

        DiscoverCommand = new RelayCommand(DiscoverAsync);
        ConnectCommand = new RelayCommand(ConnectAsync, () => SelectedRadio is not null);
        RefreshDaxCommand = new RelayCommand(RefreshDaxAsync);
        LaunchWsjtxCommand = new RelayCommand(LaunchWsjtxAsync, () => SelectedSlice is not null);
    }

    public ObservableCollection<RadioInfo> Radios { get; } = [];
    public ObservableCollection<SliceState> Slices { get; } = [];
    public ObservableCollection<DaxEndpoint> DaxEndpoints { get; } = [];

    public ICommand DiscoverCommand { get; }
    public ICommand ConnectCommand { get; }
    public ICommand RefreshDaxCommand { get; }
    public ICommand LaunchWsjtxCommand { get; }

    public RadioInfo? SelectedRadio
    {
        get => _selectedRadio;
        set => SetField(ref _selectedRadio, value);
    }

    public SliceState? SelectedSlice
    {
        get => _selectedSlice;
        set => SetField(ref _selectedSlice, value);
    }

    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public string DaxMessage
    {
        get => _daxMessage;
        set => SetField(ref _daxMessage, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task DiscoverAsync()
    {
        Status = "Discovering FlexRadio radios...";
        Radios.Clear();

        foreach (var radio in await _radioClient.DiscoverAsync())
        {
            Radios.Add(radio);
        }

        Status = Radios.Count == 0 ? "No radios found." : $"Found {Radios.Count} radio(s).";
    }

    private async Task ConnectAsync()
    {
        if (SelectedRadio is null)
        {
            return;
        }

        await _radioClient.ConnectAsync(SelectedRadio);
        Status = $"Connected to {SelectedRadio.Nickname}.";
    }

    private async Task RefreshDaxAsync()
    {
        var devices = await _audioDeviceScanner.GetAudioDevicesAsync();
        var scan = _daxDetector.Detect(devices, SelectedRadio?.SmartSdrVersion);

        if (scan.Endpoints.Count == 0 && SelectedRadio?.Serial == "DEMO0001")
        {
            scan = _daxDetector.Detect([
                new WindowsAudioDevice("demo-rx-1", "DAX RX 1 (FlexRadio DAX)"),
                new WindowsAudioDevice("demo-rx-2", "DAX RX 2 (FlexRadio DAX)"),
                new WindowsAudioDevice("demo-tx", "DAX TX (FlexRadio DAX)")
            ], SelectedRadio.SmartSdrVersion);
        }

        _latestDaxScan = scan;

        DaxEndpoints.Clear();
        foreach (var endpoint in scan.Endpoints)
        {
            DaxEndpoints.Add(endpoint);
        }

        DaxMessage = scan.Warning ?? scan.Message ?? $"Detected {scan.Endpoints.Count} DAX endpoint(s).";
    }

    private async Task LaunchWsjtxAsync()
    {
        if (SelectedSlice is null)
        {
            Status = "Select a slice before launching WSJT-X.";
            return;
        }

        if (_latestDaxScan is null)
        {
            await RefreshDaxAsync();
        }

        if (_latestDaxScan is null || _latestDaxScan.Endpoints.Count == 0)
        {
            Status = "No DAX endpoints are available. Start SmartSDR DAX and refresh devices.";
            return;
        }

        var wsjtxPath = _wsjtxInstallLocator.Find(_config.Apps.Wsjtx.Path);
        if (string.IsNullOrWhiteSpace(wsjtxPath))
        {
            Status = "WSJT-X was not found. Set apps.wsjtx.path in appsettings.json.";
            return;
        }

        var launcher = new WsjtxLauncher(wsjtxPath, _wsjtxConfigManager);
        _sessionManager ??= new SliceSessionManager(_radioClient, launcher, _wsjtxConfigManager);

        try
        {
            var session = await _sessionManager.StartAsync(new SliceSessionRequest
            {
                Slice = SelectedSlice,
                DaxScanResult = _latestDaxScan,
                SettingsRoot = _config.Apps.Wsjtx.SettingsRoot,
                BaseCatPort = _config.Apps.Wsjtx.BaseCatPort
            });

            Status = $"Started {session.SessionId} on CAT port {session.CatPort}.";
        }
        catch (Exception ex)
        {
            Status = ex.Message;
        }
    }

    private void OnSliceChanged(SliceState slice)
    {
        if (Application.Current?.Dispatcher.CheckAccess() == false)
        {
            Application.Current.Dispatcher.Invoke(() => OnSliceChanged(slice));
            return;
        }

        var existing = Slices.FirstOrDefault(item => item.SliceId == slice.SliceId);
        if (existing is null)
        {
            Slices.Add(slice);
        }
        else
        {
            var index = Slices.IndexOf(existing);
            Slices[index] = slice;
        }

        SelectedSlice ??= Slices.FirstOrDefault();
        if (_sessionManager is not null)
        {
            _ = _sessionManager.UpdateSliceAsync(slice.SliceId, slice);
        }
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        if (ConnectCommand is RelayCommand connectCommand)
        {
            connectCommand.RaiseCanExecuteChanged();
        }

        if (LaunchWsjtxCommand is RelayCommand launchCommand)
        {
            launchCommand.RaiseCanExecuteChanged();
        }
    }
}
