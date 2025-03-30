using OpenTK.Mathematics;

namespace Vint.Core.Battle.Simulations.Renderer.Objects;

public class Camera : RenderObject {
    const float LerpSpeed = 5f;
    float _fovRad;

    public float Fov {
        get => MathHelper.RadiansToDegrees(_fovRad);
        set => _fovRad = MathHelper.DegreesToRadians(value);
    }

    public Vector3 Up { get; private set; } = Vector3.UnitY;
    public Vector3 Right { get; private set; } = Vector3.UnitX;
    public Vector3 Forward { get; private set; } = -Vector3.UnitZ;

    public float AspectRatio { get; set; }
    public float Near { get; set; }
    public float Far { get; set; }
    public float PitchClamp { get; set; }

    public void AddPosition(Vector3 position, float speed) {
        Vector3 movement = Right * position.X + Vector3.UnitY * position.Y + Forward * position.Z;

        if (movement.Length != 0)
            Position += movement.Normalized() * speed;
    }

    public void AddRotation(Vector3 rotation, TimeSpan deltaTime) {
        Vector3 oldRotation = Rotation;

        Vector3 newRotation = Vector3.Lerp(oldRotation, oldRotation + rotation * InputManager.AngularSpeed, (float)(1 * deltaTime.TotalSeconds));
        newRotation.X = Math.Clamp(newRotation.X, -PitchClamp, PitchClamp);

        Rotation = newRotation;
    }

    public void UpdateDirections() {
        Forward = Vector3.Normalize(new Vector3(
            MathF.Cos(Rotation.X) * MathF.Cos(Rotation.Y),
            MathF.Sin(Rotation.X),
            MathF.Cos(Rotation.X) * MathF.Sin(Rotation.Y)));

        Right = Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Forward));
    }

    public Matrix4 GetViewMatrix() =>
        Matrix4.LookAt(Position, Position + Forward, Up);

    public Matrix4 GetProjectionMatrix() =>
        Matrix4.CreatePerspectiveFieldOfView(_fovRad, AspectRatio, Near, Far);
}
