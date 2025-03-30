using BepuPhysics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Serilog;
using Vint.Core.Battle.Simulations.Geometry;
using Vint.Core.Battle.Simulations.Renderer.Objects;
using Vint.Core.Battle.Simulations.Renderer.Shaders;
using Vint.Core.Structures;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Simulations.Renderer;

public class RendererWindow : IDisposable {
    static RendererWindow() {
        GLFWProvider.SetErrorCallback((error, description) =>
            Log.Logger.ForType(typeof(GLFWProvider)).Error("{Code}\n{Description}", error, description));
        GLFWProvider.CheckForMainThread = false;
    }

    public RendererWindow(
        string title,
        Simulation simulation
    ) {
        Simulation = simulation;

        Camera.Fov = 90;
        Camera.Near = 0.01f;
        Camera.Far = 20000f;
        Camera.PitchClamp = MathHelper.DegToRad * 89;

        Dispatcher.Invoke(() => {
            Window = new NativeWindow(GetNativeWindowSettings(title));
            Window.FramebufferResize += OnFramebufferResize;

            InputManager = new InputManager(Window.KeyboardState, Window.MouseState);

            Window.Context?.MakeCurrent();
            Shader = new Shader("shader.vert", "shader.frag");

            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);
            GL.DepthRange(0, 1);
            GL.DepthFunc(DepthFunction.Less);

            GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);

            Window.CursorState = CursorState.Grabbed;
        });
    }

    static ILogger Logger { get; } = Log.Logger.ForType<RendererWindow>();

    public Dispatcher Dispatcher { get; } = new();

    Simulation Simulation { get; }
    Camera Camera { get; } = new();
    InputManager InputManager { get; set; } = null!;
    Shader Shader { get; set; } = null!;
    NativeWindow Window { get; set; } = null!;

    Dictionary<StaticHandle, Mesh> Statics { get; } = [];
    Dictionary<BodyHandle, Mesh> Bodies { get; } = [];

    public void AddStatic(Triangle[] triangles, Vector3 scale, StaticHandle handle) => Dispatcher.Invoke(() => {
        Mesh mesh = new(triangles, scale);
        Statics.Add(handle, mesh);
    });

    public void AddBody(Triangle[] triangles, Vector3 scale, BodyHandle handle) => Dispatcher.Invoke(() => {
        Mesh mesh = new(triangles, scale);
        Bodies.Add(handle, mesh);
    });

    public async Task Tick(TimeSpan deltaTime) => await Dispatcher.InvokeAsync(() => {
        if (Window.Context is not { IsCurrent: true })
            throw new InvalidOperationException("Context is not current");

        Window.NewInputFrame();
        NativeWindow.ProcessWindowEvents(Window.IsEventDriven);

        OnUpdateFrame(deltaTime);
        OnRenderFrame(deltaTime);
    });

    void OnUpdateFrame(TimeSpan deltaTime) {
        float speed = (float)(deltaTime.TotalSeconds * InputManager.GetLinearSpeed());

        Camera.AspectRatio = Window.Size.X / (float)Window.Size.Y;
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

        Window.Context.SwapBuffers();
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

    static void OnFramebufferResize(FramebufferResizeEventArgs e) =>
        GL.Viewport(0, 0, e.Width, e.Height);

    static Matrix4 ConvertTransform(System.Numerics.Vector3 position, System.Numerics.Quaternion orientation, Vector3 scale) =>
        Matrix4.CreateFromQuaternion((Quaternion)orientation) * Matrix4.CreateTranslation((Vector3)position) * Matrix4.CreateScale(scale);

    static NativeWindowSettings GetNativeWindowSettings(string title) => new() {
        Title = title,
        ClientSize = (1280, 720),
        NumberOfSamples = 8,
        Vsync = VSyncMode.Adaptive
    };

    protected void Dispose(bool disposing) {
        if (!disposing) return;

        Dispatcher.Invoke(() => {
            Window.CursorState = CursorState.Normal;
            Shader.Dispose();
            Window.Dispose();
        });

        Dispatcher.Dispose();
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
