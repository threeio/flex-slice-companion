using System.Windows;
using FlexSliceCompanion.Core.Config;
using FlexSliceCompanion.Core.Radio;
using FlexSliceCompanion.FlexLib;
using FlexSliceCompanion.Plugins.Wsjtx;
using FlexSliceCompanion.SmartSdrApi;
using FlexSliceCompanion.Windows.Services;
using FlexSliceCompanion.Windows.ViewModels;

namespace FlexSliceCompanion.Windows;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var configStore = new JsonConfigStore();
        var configPath = ConfigPathResolver.GetUserConfigPath();
        var config = configStore.LoadAsync(configPath).GetAwaiter().GetResult();
        configStore.SaveAsync(configPath, config).GetAwaiter().GetResult();

        var radioClient = new FallbackRadioClient(
            new FlexLibRadioClient(),
            new FallbackRadioClient(new SmartSdrRadioClient(), new DemoRadioClient()));
        DataContext = new MainWindowViewModel(
            radioClient,
            new AudioDeviceScanner(),
            config,
            new WsjtxConfigManager(),
            new WsjtxInstallLocator());
    }
}
