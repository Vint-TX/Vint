using System.Collections.Frozen;
using BepuPhysics;
using BepuPhysics.Collidables;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Serilog;
using Vint.Core.Battle.Simulations.Components;
using Vint.Core.Battle.Simulations.Renderer.Objects;
using Vint.Core.Battle.Simulations.Renderer.Shaders;
using Vint.Core.Battle.Simulations.Renderer.Utils;
using Vint.Core.Config;
using Vint.Core.Structures;
using Vint.Core.Utils;
using Mesh = Vint.Core.Battle.Simulations.Renderer.Objects.Mesh;
using Triangle = Vint.Core.Battle.Simulations.Geometry.Triangle;
using BepuMesh = BepuPhysics.Collidables.Mesh;

namespace Vint.Core.Battle.Simulations.Renderer;

public class RendererWindow : IDisposable {
    static RendererWindow() {
        GLFWProvider.SetErrorCallback((error, description) =>
            Log.Logger.ForType(typeof(GLFWProvider)).Error("{Code}\n{Description}", error, description));
        GLFWProvider.CheckForMainThread = false;
    }

    public RendererWindow(
        string title,
        Simulation simulation,
        Dispatcher dispatcher
    ) {
        Dispatcher = dispatcher;
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
        });
    }

    static ILogger Logger { get; } = Log.Logger.ForType<RendererWindow>();
    static FrozenDictionary<string, string> Colors { get; } =
        ConfigManager.GetComponent<ColorConfigComponent>("battle/simulation/renderer").Colors.ToFrozenDictionary();

    Dispatcher Dispatcher { get; }
    Simulation Simulation { get; }
    Camera Camera { get; } = new();
    InputManager InputManager { get; set; } = null!;
    Shader Shader { get; set; } = null!;
    NativeWindow Window { get; set; } = null!;
    Vector3 LightColor { get; } = Vector3.One * 0.3f;

    Dictionary<StaticHandle, Mesh> Statics { get; } = [];
    Dictionary<BodyHandle, Mesh> Bodies { get; } = [];

    public void AddStatic(string name, string? colorName, Triangle[] triangles, StaticHandle handle) => Dispatcher.Invoke(() => {
        Mesh mesh = new(name, triangles, GetColor(colorName), Shader);
        Statics.Add(handle, mesh);
    });

    public void AddBody(string name, string? colorName, Triangle[] triangles, BodyHandle handle) => Dispatcher.Invoke(() => {
        Mesh mesh = new(name, triangles, GetColor(colorName), Shader);
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
        Camera.AspectRatio = Window.Size.X / (float)Window.Size.Y;

        if (Window.CursorState == CursorState.Grabbed) {
            float speed = (float)(deltaTime.TotalSeconds * InputManager.GetLinearSpeed());

            Camera.AddPosition(InputManager.MovementVector, speed);
            Camera.AddRotation(InputManager.MouseVector, deltaTime);
            Camera.UpdateDirections();
        }

        if (InputManager.IsButtonPressed(MouseButton.Left))
            Window.CursorState = CursorState.Grabbed;
        else if (InputManager.IsKeyPressed(Keys.Escape))
            Window.CursorState = CursorState.Normal;
    }

    void OnRenderFrame(TimeSpan deltaTime) {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        Shader.Use();
        Shader.SetMatrix4("view", Camera.GetViewMatrix());
        Shader.SetMatrix4("projection", Camera.GetProjectionMatrix());
        Shader.SetVector3("viewPos", Camera.Position);
        Shader.SetVector3("lightColor", LightColor);

        RenderStatics();
        RenderBodies();

        Window.Context.SwapBuffers();
    }

    void RenderStatics() {
        foreach ((StaticHandle handle, Mesh mesh) in Statics) {
            StaticReference reference = Simulation.Statics[handle];

            ref RigidPose pose = ref reference.Pose;
            ref BepuMesh bepuMesh = ref Simulation.Shapes.GetShape<BepuMesh>(reference.Shape.Index);

            Matrix4 model = ConvertTransform((Vector3)pose.Position, (Quaternion)pose.Orientation, (Vector3)bepuMesh.Scale);
            Shader.SetMatrix4("model", model);

            mesh.Draw();
        }
    }

    void RenderBodies() {
        foreach ((BodyHandle handle, Mesh value) in Bodies) {
            BodyReference reference = Simulation.Bodies[handle];

            ref RigidPose pose = ref reference.Pose;
            ref Collidable collidable = ref reference.Collidable;
            ref BepuMesh bepuMesh = ref Simulation.Shapes.GetShape<BepuMesh>(collidable.Shape.Index);

            Matrix4 model = ConvertTransform((Vector3)pose.Position, (Quaternion)pose.Orientation, (Vector3)bepuMesh.Scale);
            Shader.SetMatrix4("model", model);

            value.Draw();
        }
    }

    static Vector3 GetColor(string? colorName) =>
        !string.IsNullOrWhiteSpace(colorName) && Colors.TryGetValue(colorName, out string? hex)
            ? ColorUtils.HexToRgb(hex)
            : Vector3.One;

    static void OnFramebufferResize(FramebufferResizeEventArgs e) =>
        GL.Viewport(0, 0, e.Width, e.Height);

    static Matrix4 ConvertTransform(Vector3 position, Quaternion orientation, Vector3 scale) =>
        Matrix4.CreateFromQuaternion(orientation) * Matrix4.CreateTranslation(position) * Matrix4.CreateScale(scale);

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
