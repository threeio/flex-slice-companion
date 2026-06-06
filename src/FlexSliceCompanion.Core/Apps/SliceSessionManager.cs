using FlexSliceCompanion.Core.Audio;
using FlexSliceCompanion.Core.Cat;
using FlexSliceCompanion.Core.Radio;

namespace FlexSliceCompanion.Core.Apps;

public sealed class SliceSessionManager : IAsyncDisposable
{
    private readonly IRadioClient _radioClient;
    private readonly IExternalRadioApp _externalApp;
    private readonly IAppSettingsDirectoryResolver _settingsDirectoryResolver;
    private readonly DaxEndpointSelector _daxEndpointSelector = new();
    private readonly Dictionary<string, CatServer> _catServers = [];
    private readonly Dictionary<string, ManagedSliceSession> _sessions = [];

    public SliceSessionManager(
        IRadioClient radioClient,
        IExternalRadioApp externalApp,
        IAppSettingsDirectoryResolver settingsDirectoryResolver)
    {
        _radioClient = radioClient;
        _externalApp = externalApp;
        _settingsDirectoryResolver = settingsDirectoryResolver;
    }

    public IReadOnlyCollection<ManagedSliceSession> Sessions => _sessions.Values;

    public async Task<ManagedSliceSession> StartAsync(SliceSessionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (_sessions.ContainsKey(request.Slice.SliceId))
        {
            await StopAsync(request.Slice.SliceId, cancellationToken);
        }

        if (!_externalApp.IsInstalled())
        {
            throw new InvalidOperationException($"{_externalApp.Name} is not installed or its path is not configured.");
        }

        var dax = _daxEndpointSelector.SelectForSlice(
            request.DaxScanResult,
            request.Slice.DaxChannel,
            request.PreferredDaxGeneration);

        var catPort = request.BaseCatPort + SliceOffset(request.Slice.Letter);
        var settingsDirectory = _settingsDirectoryResolver.ResolveSettingsDirectory(request.SettingsRoot, request.Slice.Letter);
        var launchRequest = new AppLaunchRequest
        {
            Slice = request.Slice,
            CatPort = catPort,
            RxAudioEndpoint = dax.Rx,
            TxAudioEndpoint = dax.Tx,
            SettingsDirectory = settingsDirectory
        };

        var appLaunch = await _externalApp.LaunchAsync(launchRequest, cancellationToken);
        var catServer = new CatServer(new CatServerOptions { SliceId = request.Slice.SliceId, Port = catPort }, _radioClient);
        await catServer.UpdateSliceAsync(request.Slice, cancellationToken);

        _catServers[request.Slice.SliceId] = catServer;
        _ = Task.Run(() => catServer.StartAsync(cancellationToken), cancellationToken);

        var session = new ManagedSliceSession
        {
            SessionId = appLaunch.InstanceId,
            Slice = request.Slice,
            CatPort = catPort,
            DaxEndpoints = dax,
            AppLaunch = appLaunch
        };

        _sessions[request.Slice.SliceId] = session;
        return session;
    }

    public async Task UpdateSliceAsync(string sliceId, FlexSliceCompanion.Core.Slices.SliceState slice, CancellationToken cancellationToken = default)
    {
        if (_catServers.TryGetValue(sliceId, out var server))
        {
            await server.UpdateSliceAsync(slice, cancellationToken);
        }
    }

    public async Task StopAsync(string sliceId, CancellationToken cancellationToken = default)
    {
        if (_sessions.TryGetValue(sliceId, out var session) && session.AppLaunch is not null)
        {
            await _externalApp.StopAsync(session.AppLaunch.InstanceId, cancellationToken);
        }

        if (_catServers.Remove(sliceId, out var server))
        {
            await server.DisposeAsync();
        }

        _sessions.Remove(sliceId);
    }

    private static int SliceOffset(string letter)
    {
        if (string.IsNullOrWhiteSpace(letter))
        {
            return 1;
        }

        var first = char.ToUpperInvariant(letter[0]);
        return first is >= 'A' and <= 'Z' ? first - 'A' + 1 : 1;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var sliceId in _sessions.Keys.ToArray())
        {
            await StopAsync(sliceId);
        }
    }
}
