using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Serilog;
using Vint.Core.Config;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Simulations.Renderer.Shaders;

public class Shader : IShader, IDisposable {
    bool _disposed;

    static ILogger Logger { get; } = Log.Logger.ForType<Shader>();

    int Handle { get; }

    public Shader(string vertexName, string fragmentName) {
        Handle = GL.CreateProgram();

        int vertex = PrepareShader(vertexName, ShaderType.VertexShader);
        int fragment = PrepareShader(fragmentName, ShaderType.FragmentShader);

        GL.AttachShader(Handle, vertex);
        GL.AttachShader(Handle, fragment);

        GL.LinkProgram(Handle);
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int status);

        if (status == 0) {
            string infoLog = GL.GetProgramInfoLog(Handle);
            Logger.Error(infoLog);
        }

        ClearShaders(Handle, vertex, fragment);
    }

    public void Use() => GL.UseProgram(Handle);

    public void SetMatrix4(string name, Matrix4 matrix) {
        int location = GL.GetUniformLocation(Handle, name);

        GL.UniformMatrix4(location, true, ref matrix);
    }

    public void SetVector3(string name, Vector3 vector) {
        int location = GL.GetUniformLocation(Handle, name);

        GL.Uniform3(location, ref vector);
    }

    static string GetShaderPath(string shaderName) => Path.Combine(ConfigManager.ResourcesPath, "Simulation", "Renderer", "Shaders", shaderName);

    static int PrepareShader(string name, ShaderType type) {
        string source = File.ReadAllText(GetShaderPath(name));
        int shader = GL.CreateShader(type);

        GL.ShaderSource(shader, source);

        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);

        if (status != 0) return shader;

        GL.GetShaderInfoLog(shader, out string infoLog);
        Logger.Error(infoLog);
        return shader;
    }

    static void ClearShaders(int handle, params int[] shaders) {
        foreach (int shader in shaders) {
            GL.DetachShader(handle, shader);
            GL.DeleteShader(shader);
        }
    }

    protected void Dispose(bool disposing) {
        if (_disposed) return;

        GL.DeleteProgram(Handle);
        _disposed = true;
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
