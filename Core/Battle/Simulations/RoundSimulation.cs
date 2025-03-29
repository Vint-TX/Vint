using BepuPhysics;
using BepuUtilities;
using Vint.Core.Battle.Rounds;
using Vint.Core.Battle.Simulations.Renderer;

namespace Vint.Core.Battle.Simulations;

public class RoundSimulation : IDisposable {
    bool _firstTick = true;

    public RoundSimulation(Round round) {
        Round = round;

        Simulation = new Simulation();
        ThreadDispatcher = new ThreadDispatcher(Environment.ProcessorCount);

#if DEBUG
        Renderer = new RendererWindow($"Simulation {round.Entity.Id}", Simulation);
#endif
    }

    public Round Round { get; }
    public Simulation Simulation { get; }
    public RendererWindow? Renderer { get; }

    IThreadDispatcher? ThreadDispatcher { get; }

    public void Tick(TimeSpan deltaTime) {
        if (_firstTick) {
            Renderer?.OnLoad(); // needs to be called from update thread
            _firstTick = false;
        }

        Simulation.Timestep((float)deltaTime.TotalSeconds, ThreadDispatcher);
        Renderer?.Tick(deltaTime);
    }

    public void Dispose() {
        Simulation.Dispose();
        Renderer?.Dispose();

        GC.SuppressFinalize(this);
    }
}
