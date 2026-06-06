using FlexSliceCompanion.Core.Audio;
using NAudio.CoreAudioApi;

namespace FlexSliceCompanion.Windows.Services;

public sealed class AudioDeviceScanner
{
    public Task<IReadOnlyList<WindowsAudioDevice>> GetAudioDevicesAsync()
    {
        using var enumerator = new MMDeviceEnumerator();
        var devices = new List<WindowsAudioDevice>();

        foreach (var dataFlow in new[] { DataFlow.Capture, DataFlow.Render })
        {
            foreach (var device in enumerator.EnumerateAudioEndPoints(dataFlow, DeviceState.Active))
            {
                devices.Add(new WindowsAudioDevice(device.ID, device.FriendlyName));
                device.Dispose();
            }
        }

        return Task.FromResult<IReadOnlyList<WindowsAudioDevice>>(
            devices
                .GroupBy(device => device.Id)
                .Select(group => group.First())
                .OrderBy(device => device.DisplayName)
                .ToArray());
    }
}
