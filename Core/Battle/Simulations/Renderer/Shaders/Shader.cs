using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Serilog;
using Vint.Core.Config;
using Vint.Core.Utils;

namespace Vint.Core.Battle.Simulations.Renderer.Shaders;

public class Shader : IDisposable {
    readonly int _handle;
    bool _disposed;

    static ILogger Logger { get; } = Log.Logger.ForType<Shader>();

    static string GetShaderPath(string shaderName) => Path.Combine(ConfigManager.ResourcesPath, "Renderer", "Shaders", shaderName);

    public Shader(string vertexName, string fragmentName) {
        _handle = GL.CreateProgram();

        int vertex = PrepareShader(vertexName, ShaderType.VertexShader);
        int fragment = PrepareShader(fragmentName, ShaderType.FragmentShader);

        GL.AttachShader(_handle, vertex);
        GL.AttachShader(_handle, fragment);

        GL.LinkProgram(_handle);
        GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out int status);

        if (status == 0) {
            string infoLog = GL.GetProgramInfoLog(_handle);
            Logger.Error(infoLog);
        }

        ClearShaders(_handle, vertex, fragment);
    }

    public void Use() => GL.UseProgram(_handle);

    public void SetMatrix4(string name, Matrix4 matrix) {
        int location = GL.GetUniformLocation(_handle, name);

        GL.UniformMatrix4(location, true, ref matrix);
    }

    public void SetVector3(string name, Vector3 vector) {
        int location = GL.GetUniformLocation(_handle, name);

        GL.Uniform3(location, ref vector);
    }

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

        GL.DeleteProgram(_handle);
        _disposed = true;
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
