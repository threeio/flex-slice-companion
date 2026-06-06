using System.Windows;
using FlexSliceCompanion.FlexLib;
using FlexSliceCompanion.Windows.Services;
using FlexSliceCompanion.Windows.ViewModels;

namespace FlexSliceCompanion.Windows;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(new FlexLibRadioClient(), new AudioDeviceScanner());
    }
}
