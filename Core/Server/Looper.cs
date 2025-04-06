using System.Diagnostics;
using Serilog;
using Vint.Core.Utils;

namespace Vint.Core.Server;

public class Looper(
    Func<TimeSpan, Task> onTick,
    int tickRate,
    CancellationToken cancellationToken = default
) {
    const int MaxSlowUpdates = 80;
    const int SlowUpdatesThreshold = 45;

    int _slowUpdates;

    ILogger Logger { get; } = Log.Logger.ForType<Looper>();
    public TimeSpan DeltaTime { get; private set; }
    public bool IsRunningSlowly { get; private set; }

    public async Task RunAsync() { // https://stackoverflow.com/q/78850638
        if (cancellationToken.IsCancellationRequested)
            return;

        TimeSpan targetDeltaTime = TimeSpan.FromSeconds(1d / tickRate);
        Stopwatch stopwatch = Stopwatch.StartNew();

        while (!cancellationToken.IsCancellationRequested) {
            TimeSpan elapsed = stopwatch.Elapsed;

            if (elapsed > targetDeltaTime) {
                stopwatch.Restart();
                DeltaTime = elapsed;

                try {
                    await onTick(DeltaTime);
                } catch (Exception e) {
                    Logger.Error(e, "Caught an exception");
                }

                TimeSpan time = stopwatch.Elapsed;
                if (targetDeltaTime < time) {
                    _slowUpdates++;
                    if (_slowUpdates > MaxSlowUpdates)
                        _slowUpdates = MaxSlowUpdates;
                } else {
                    _slowUpdates--;
                    if (_slowUpdates < 0)
                        _slowUpdates = 0;
                }

                bool wasRunningSlowly = IsRunningSlowly;
                IsRunningSlowly = _slowUpdates > SlowUpdatesThreshold;

                switch (IsRunningSlowly) {
                    case true when !wasRunningSlowly:
                        Logger.Warning("Game loop is running slowly. TPS: {Tps}, DeltaTime: {DeltaTime}", 1 / DeltaTime.TotalSeconds, DeltaTime);
                        break;

                    case false when wasRunningSlowly:
                        Logger.Information("Game loop is back to normal. TPS: {Tps}, DeltaTime: {DeltaTime}", 1 / DeltaTime.TotalSeconds, DeltaTime);
                        break;
                }
            }

            if (cancellationToken.IsCancellationRequested)
                break;

            TimeSpan freeTime = targetDeltaTime - stopwatch.Elapsed;

            if (freeTime > TimeSpan.Zero)
                await Task.Delay(freeTime);
        }
    }
}
