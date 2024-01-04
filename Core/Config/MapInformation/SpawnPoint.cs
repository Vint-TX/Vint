using System.Numerics;

namespace Vint.Core.Config.MapInformation;

public class SpawnPoint {
    public int Number { get; set; } = 50;
    public Vector3 Position { get; set; } = new(0, 8, 0);
    public Quaternion Rotation { get; set; } = new();
}