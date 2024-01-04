using System.Numerics;

namespace Vint.Core.Config.MapInformation;

public class TeleportPoint(
    string name,
    Vector3 position,
    Quaternion rotation
) {
    public string Name { get; set; } = name;
    public Vector3 Position { get; set; } = position;
    public Quaternion Rotation { get; set; } = rotation;
}