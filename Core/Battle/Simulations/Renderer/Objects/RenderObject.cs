using OpenTK.Mathematics;

namespace Vint.Core.Battle.Simulations.Renderer.Objects;

public abstract class RenderObject {
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public Vector3 Scale { get; set; } = Vector3.One;

    public Matrix4 GetMatrix() =>
        Matrix4.CreateScale(new Vector3(Scale.X, Scale.Y, Scale.Z)) *
        Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotation.X)) *
        Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotation.Y)) *
        Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation.Z)) *
        Matrix4.CreateTranslation(new Vector3(Position.X, Position.Y, Position.Z));
}
