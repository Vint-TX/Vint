using BepuPhysics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Vint.Core.Battle.Simulations.Renderer.Objects;
using Vint.Core.Battle.Simulations.Renderer.Shaders;

namespace Vint.Core.Battle.Simulations.Renderer;

public class RendererWindow : NativeWindow {
    public RendererWindow(string title, Simulation simulation) : base(GetNativeWindowSettings(title)) {
        InputManager = new InputManager(KeyboardState, MouseState);
        Simulation = simulation;

        Camera.Fov = 90;
        Camera.Near = 0.01f;
        Camera.Far = 20000f;
        Camera.PitchClamp = MathHelper.DegToRad * 89;
    }

    Simulation Simulation { get; }
    InputManager InputManager { get; }
    Shader Shader { get; } = new("shader.vert", "shader.frag");
    Camera Camera { get; } = new();

    public void OnLoad() {
        Context?.MakeCurrent();

        GL.Enable(EnableCap.DepthTest);
        GL.DepthMask(true);
        GL.DepthRange(0, 1);
        GL.DepthFunc(DepthFunction.Less);

        GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);

        // GameObject tank = new();
        // _tank = tank.AddComponent<MeshRenderer>();
        // _tank.SetMesh(MeshParser.Parse(ResourcesManager.GetResourcePath("Models/moon_silence.glb")));

        CursorState = CursorState.Grabbed;
    }

    public void Tick(TimeSpan deltaTime) {
        NewInputFrame();
        ProcessWindowEvents(IsEventDriven);

        OnUpdateFrame(deltaTime);
        OnRenderFrame(deltaTime);
    }

    void OnUpdateFrame(TimeSpan deltaTime) {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        float speed = (float)(deltaTime.TotalSeconds * InputManager.GetLinearSpeed());

        Camera.AspectRatio = Size.X / (float)Size.Y;
        Camera.AddPosition(InputManager.MovementVector, speed);
        Camera.AddRotation(InputManager.MouseVector, deltaTime);
        Camera.UpdateDirections();

        // Shader.Use();
        // Shader.SetMatrix4("model", _tank.Transform.GetMatrix());
        // Shader.SetMatrix4("view", Camera.GetViewMatrix());
        // Shader.SetMatrix4("projection", Camera.GetProjectionMatrix());
        // Shader.SetVector3("viewPos", Camera.Transform.Position);
        // Shader.SetVector3("lightColor", Vector3.One * 0.3f);
        // _tank.Draw();

        Context.SwapBuffers();
    }

    void OnRenderFrame(TimeSpan deltaTime) { }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e) {
        base.OnFramebufferResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (!disposing) return;

        CursorState = CursorState.Normal;
        Shader.Dispose();
    }

    static NativeWindowSettings GetNativeWindowSettings(string title) => new() {
        Title = title,
        ClientSize = (1280, 720),
        NumberOfSamples = 8,
        Vsync = VSyncMode.Adaptive
    };
}
