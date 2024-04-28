namespace Vint.Core.Battles.Player;

public class PerformanceStatisticData {
    public string UserName { get; set; }

    public string GraphicDeviceName { get; set; }

    public string GraphicsDeviceType { get; set; }

    public int GraphicsMemorySize { get; set; }

    public string DefaultQuality { get; set; }

    public string Quality { get; set; }

    public string Resolution { get; set; }

    public string MapName { get; set; }

    public int BattleRoundTimeInMin { get; set; }

    public int TankCountModa { get; set; }

    public int Moda { get; set; }

    public int Average { get; set; }

    public int StandardDeviationInMs { get; set; }

    public int MinAverageForInterval { get; set; }

    public int MaxAverageForInterval { get; set; }

    public int HugeFrameCount { get; set; }

    public string GraphicDeviceKey { get; set; }

    public string GraphicsDeviceVersion { get; set; }

    public int AveragePing { get; set; }

    public bool CustomSettings { get; set; }

    public bool Windowed { get; set; }

    public float SaturationLevel { get; set; }

    public int VegetationLevel { get; set; }

    public int GrassLevel { get; set; }

    public int AntialiasingQuality { get; set; }

    public int AnisotropicQuality { get; set; }

    public int TextureQuality { get; set; }

    public int ShadowQuality { get; set; }

    public bool AmbientOcclusion { get; set; }

    public bool Bloom { get; set; }

    public int RenderResolutionQuality { get; set; }

    public int PingModa { get; set; }

    public int SystemMemorySize { get; set; }

    public long TotalReservedMemory { get; set; }

    public long TotalAllocatedMemory { get; set; }

    public long MonoHeapSize { get; set; }

    public string[] HandlerNames { get; set; }

    public int[] HandlerCallCounts { get; set; }
}
