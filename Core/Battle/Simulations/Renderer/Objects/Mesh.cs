using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Vint.Core.Battle.Simulations.Geometry;

namespace Vint.Core.Battle.Simulations.Renderer.Objects;

public class Mesh : RenderObject, IDisposable {
    readonly int _vertexBufferId;
    readonly int _vertexArrayId;

    readonly Triangle[] _triangles;

    public Mesh(Triangle[] triangles, Vector3 scale) {
        Scale = scale;
        _triangles = triangles;

        _vertexBufferId = GL.GenBuffer();
        _vertexArrayId = GL.GenVertexArray();

        int triangleSize = Marshal.SizeOf<Triangle>();

        GL.BindVertexArray(_vertexArrayId);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferId);
        GL.BufferData(BufferTarget.ArrayBuffer,
            _triangles.Length * triangleSize,
            _triangles,
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
        GL.BindVertexArray(_vertexArrayId);
        GL.DrawArrays(PrimitiveType.Triangles, 0, _triangles.Length * 3);
        GL.BindVertexArray(0);
    }

    public void Dispose() {
        GL.DeleteVertexArray(_vertexArrayId);
        GL.DeleteBuffer(_vertexBufferId);
        GC.SuppressFinalize(this);
    }
}
