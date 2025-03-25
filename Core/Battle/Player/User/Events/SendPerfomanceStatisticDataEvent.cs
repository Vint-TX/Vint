using Vint.Core.ECS.Entities;
using Vint.Core.Server.Game;
using Vint.Core.Server.Game.Protocol.Attributes;

namespace Vint.Core.ECS.Events.Battle;

[ProtocolId(1470658766256)]
public class SendPerformanceStatisticDataEvent : IServerEvent { // todo: what the fuck am i supposed to do with this information?
    public PerformanceStatisticData Data { get; set; } = null!;

    public Task Execute(IPlayerConnection connection, IEntity[] entities) => Task.CompletedTask;
}

public class PerformanceStatisticData {
    public string UserName { get; private set; } = null!;

    public string GraphicDeviceName { get; private set; } = null!;

    public string GraphicsDeviceType { get; private set; } = null!;

    public int GraphicsMemorySize { get; private set; }

    public string DefaultQuality { get; private set; } = null!;

    public string Quality { get; private set; } = null!;

    public string Resolution { get; private set; } = null!;

    public string MapName { get; private set; } = null!;

    public int BattleRoundTimeInMin { get; private set; }

    public int TankCountModa { get; private set; }

    public int Moda { get; private set; }

    public int Average { get; private set; }

    public int StandardDeviationInMs { get; private set; }

    public int MinAverageForInterval { get; private set; }

    public int MaxAverageForInterval { get; private set; }

    public int HugeFrameCount { get; private set; }

    public string GraphicDeviceKey { get; private set; } = null!;

    public string GraphicsDeviceVersion { get; private set; } = null!;

    public int AveragePing { get; private set; }

    public bool CustomSettings { get; private set; }

    public bool Windowed { get; private set; }

    public float SaturationLevel { get; private set; }

    public int VegetationLevel { get; private set; }

    public int GrassLevel { get; private set; }

    public int AntialiasingQuality { get; private set; }

    public int AnisotropicQuality { get; private set; }

    public int TextureQuality { get; private set; }

    public int ShadowQuality { get; private set; }

    public bool AmbientOcclusion { get; private set; }

    public bool Bloom { get; private set; }

    public int RenderResolutionQuality { get; private set; }

    public int PingModa { get; private set; }

    public int SystemMemorySize { get; private set; }

    public long TotalReservedMemory { get; private set; }

    public long TotalAllocatedMemory { get; private set; }

    public long MonoHeapSize { get; private set; }

    public string[] HandlerNames { get; private set; } = null!;

    public int[] HandlerCallCounts { get; private set; } = null!;
}
