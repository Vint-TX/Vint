using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Vint.Core.Battle.Simulations.Renderer;

public class InputManager(
    KeyboardState keyboardInputs,
    MouseState mouseInputs
) {
    const float BaseSpeed = 15f;
    const float SpeedUp = 2f;
    const float SlowDown = 0.5f;

    public const float AngularSpeed = 1f;

    public Vector3 MovementVector {
        get {
            int xAxis = IsKeyDown(Keys.D) ? 1 : IsKeyDown(Keys.A) ? -1 : 0;
            int yAxis = IsKeyDown(Keys.R) ? 1 : IsKeyDown(Keys.F) ? -1 : 0;
            int zAxis = IsKeyDown(Keys.W) ? 1 : IsKeyDown(Keys.S) ? -1 : 0;

            return new Vector3(xAxis, yAxis, zAxis);
        }
    }

    public Vector3 MouseVector => new(-mouseInputs.Delta.Y, mouseInputs.Delta.X, 0f);

    public bool IsSpeedUp => IsKeyDown(Keys.LeftShift) || IsKeyDown(Keys.RightShift);
    public bool IsSlowDown => IsKeyDown(Keys.LeftControl) || IsKeyDown(Keys.RightControl);

    public bool IsKeyDown(Keys key) => keyboardInputs.IsKeyDown(key);
    public bool IsKeyPressed(Keys key) => keyboardInputs.IsKeyPressed(key);

    public bool IsButtonPressed(MouseButton button) => mouseInputs.IsButtonPressed(button);

    public float GetLinearSpeed() {
        float speed = BaseSpeed;

        if (IsSpeedUp) speed *= SpeedUp;
        else if (IsSlowDown) speed *= SlowDown;

        return speed;
    }
}
