using BepuPhysics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Serilog;
using Vint.Core.Battle.Simulations.Renderer.Objects;
using Vint.Core.Battle.Simulations.Renderer.Shaders;
using Vint.Core.Structures;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Simulations.Renderer;

public class RendererWindow : NativeWindow {
    bool _firstTick = true;

    static RendererWindow() {
        GLFWProvider.SetErrorCallback((error, description) =>
            Log.Logger.ForType(typeof(GLFWProvider)).Error("{Code}\n{Description}", error, description));
        GLFWProvider.CheckForMainThread = false;
    }

    public RendererWindow(
        string title,
        Simulation simulation,
        Dictionary<StaticHandle, Mesh> statics,
        Dictionary<BodyHandle, Mesh> bodies
    ) : base(GetNativeWindowSettings(title)) {
        InputManager = new InputManager(KeyboardState, MouseState);
        Simulation = simulation;
        Statics = statics;
        Bodies = bodies;

        Camera.Fov = 90;
        Camera.Near = 0.01f;
        Camera.Far = 20000f;
        Camera.PitchClamp = MathHelper.DegToRad * 89;
    }

    static ILogger Logger { get; } = Log.Logger.ForType<RendererWindow>();

    public Dispatcher Dispatcher { get; } = new();

    Simulation Simulation { get; }
    InputManager InputManager { get; }
    Shader Shader { get; set; } = null!;
    Camera Camera { get; } = new();

    Dictionary<StaticHandle, Mesh> Statics { get; }
    Dictionary<BodyHandle, Mesh> Bodies { get; }

    public void OnLoad() {
        Context?.MakeCurrent();
        Shader = new Shader("shader.vert", "shader.frag");

        GL.Enable(EnableCap.DepthTest);
        GL.DepthMask(true);
        GL.DepthRange(0, 1);
        GL.DepthFunc(DepthFunction.Less);

        GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);

        CursorState = CursorState.Grabbed;
    }

    public async Task Tick(TimeSpan deltaTime) => await Dispatcher.InvokeAsync(() => {
        if (_firstTick) {
            OnLoad();
            _firstTick = false;
        }

        if (Context is not { IsCurrent: true })
            throw new InvalidOperationException("Context is not current");

        NewInputFrame();
        ProcessWindowEvents(IsEventDriven);

        OnUpdateFrame(deltaTime);
        OnRenderFrame(deltaTime);
    });

    void OnUpdateFrame(TimeSpan deltaTime) {
        float speed = (float)(deltaTime.TotalSeconds * InputManager.GetLinearSpeed());

        Camera.AspectRatio = Size.X / (float)Size.Y;
        Camera.AddPosition(InputManager.MovementVector, speed);
        Camera.AddRotation(InputManager.MouseVector, deltaTime);
        Camera.UpdateDirections();
    }

    void OnRenderFrame(TimeSpan deltaTime) {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        Shader.Use();
        Shader.SetMatrix4("view", Camera.GetViewMatrix());
        Shader.SetMatrix4("projection", Camera.GetProjectionMatrix());

        RenderStatics();
        RenderBodies();

        Context.SwapBuffers();
    }

    void RenderStatics() {
        foreach ((StaticHandle handle, Mesh value) in Statics) {
            ref RigidPose pose = ref Simulation.Statics[handle].Pose;
            Matrix4 model = ConvertTransform(pose.Position, pose.Orientation, value.Scale);
            Shader.SetMatrix4("model", model);

            value.Draw();
        }
    }

    void RenderBodies() {
        foreach ((BodyHandle handle, Mesh value) in Bodies) {
            ref RigidPose pose = ref Simulation.Bodies[handle].Pose;
            Matrix4 model = ConvertTransform(pose.Position, pose.Orientation, value.Scale);
            Shader.SetMatrix4("model", model);

            value.Draw();
        }
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e) {
        base.OnFramebufferResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    static Matrix4 ConvertTransform(System.Numerics.Vector3 position, System.Numerics.Quaternion orientation, Vector3 scale) =>
        Matrix4.CreateFromQuaternion((Quaternion)orientation) * Matrix4.CreateTranslation((Vector3)position) * Matrix4.CreateScale(scale);

    static NativeWindowSettings GetNativeWindowSettings(string title) => new() {
        Title = title,
        ClientSize = (1280, 720),
        NumberOfSamples = 8,
        Vsync = VSyncMode.Adaptive
    };

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (!disposing) return;

        CursorState = CursorState.Normal;
        Shader.Dispose();
        Dispatcher.Dispose();
    }
}
