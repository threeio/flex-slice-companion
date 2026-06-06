using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FlexSliceCompanion.Core.Audio;
using FlexSliceCompanion.Core.Radio;
using FlexSliceCompanion.Core.Slices;
using FlexSliceCompanion.Windows.Services;

namespace FlexSliceCompanion.Windows.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly IRadioClient _radioClient;
    private readonly AudioDeviceScanner _audioDeviceScanner;
    private readonly DaxEndpointDetector _daxDetector = new();
    private RadioInfo? _selectedRadio;
    private SliceState? _selectedSlice;
    private string _status = "Ready";
    private string _daxMessage = string.Empty;

    public MainWindowViewModel(IRadioClient radioClient, AudioDeviceScanner audioDeviceScanner)
    {
        _radioClient = radioClient;
        _audioDeviceScanner = audioDeviceScanner;
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

        Status = Radios.Count == 0 ? "No radios found. FlexLib wiring is the next Windows integration step." : $"Found {Radios.Count} radio(s).";
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
        var scan = _daxDetector.Detect(devices);

        DaxEndpoints.Clear();
        foreach (var endpoint in scan.Endpoints)
        {
            DaxEndpoints.Add(endpoint);
        }

        DaxMessage = scan.Warning ?? scan.Message ?? $"Detected {scan.Endpoints.Count} DAX endpoint(s).";
    }

    private Task LaunchWsjtxAsync()
    {
        Status = SelectedSlice is null
            ? "Select a slice before launching WSJT-X."
            : $"WSJT-X launch plumbing is ready for slice {SelectedSlice.Letter}; configure the WSJT-X path next.";
        return Task.CompletedTask;
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
