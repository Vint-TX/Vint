using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Vint.Core.Battle.Simulations.Geometry;
using Vint.Core.Battle.Simulations.Renderer.Shaders;

namespace Vint.Core.Battle.Simulations.Renderer.Objects;

public class Mesh : IRenderObject, IDisposable {
    int VertexBufferId { get; }
    int VertexArrayId { get; }

    string Name { get; }

    Triangle[] Triangles { get; }
    IShader Shader { get; }
    Vector3 Color { get; }

    public Mesh(string name, Triangle[] triangles, Vector3 color, IShader shader) {
        Name = name;
        Triangles = triangles;
        Shader = shader;
        Color = color;

        VertexBufferId = GL.GenBuffer();
        VertexArrayId = GL.GenVertexArray();

        int triangleSize = Marshal.SizeOf<Triangle>();

        GL.BindVertexArray(VertexArrayId);
        GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferId);
        GL.BufferData(BufferTarget.ArrayBuffer,
            Triangles.Length * triangleSize,
            Triangles,
            BufferUsageHint.StaticDraw);

        int normalOffset = (int)Marshal.OffsetOf<Vertex>("Normal");
        int vertexSize = Marshal.SizeOf<Vertex>();

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, true, vertexSize, normalOffset);

        GL.BindVertexArray(0);
    }

    public void Draw() {
        Shader.SetVector3("objectColor", Color);

        GL.BindVertexArray(VertexArrayId);
        GL.DrawArrays(PrimitiveType.Triangles, 0, Triangles.Length * 3);
        GL.BindVertexArray(0);
    }

    public void Dispose() {
        GL.DeleteVertexArray(VertexArrayId);
        GL.DeleteBuffer(VertexBufferId);
        GC.SuppressFinalize(this);
    }
}
