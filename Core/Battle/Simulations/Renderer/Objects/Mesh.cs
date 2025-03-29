using OpenTK.Graphics.OpenGL4;

namespace Vint.Core.Battle.Simulations.Renderer.Objects;

public class Mesh : RenderObject, IDisposable {
    int _vertexBufferId;
    int _vertexArrayId;

    public void SetMesh(Mesh mesh) {
        // _mesh = mesh;
        // _vertexBufferId = GL.GenBuffer();
        // _vertexArrayId = GL.GenVertexArray();
        //
        // int triangleSize = Marshal.SizeOf<Triangle>();
        //
        // GL.BindVertexArray(_vertexArrayId);
        // GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferId);
        // GL.BufferData(BufferTarget.ArrayBuffer,
        //     mesh.Triangles.Length * triangleSize,
        //     mesh.Triangles,
        //     BufferUsageHint.StaticDraw);
        //
        // /*int uvOffset = (int) Marshal.OffsetOf<Vertex>("Uv");*/
        // int normalOffset = (int)Marshal.OffsetOf<Vertex>("Normal");
        // int vertexSize = Marshal.SizeOf<Vertex>(); // Используйте размер вершины для stride
        //
        // GL.EnableVertexAttribArray(0);
        // GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, 0);
        // /*GL.EnableVertexAttribArray(1);
        // GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, vertexSize, uvOffset);*/
        // GL.EnableVertexAttribArray(1);
        // GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, true, vertexSize, normalOffset);
        //
        // GL.BindVertexArray(0);
    }

    public void Draw() {
        GL.BindVertexArray(_vertexArrayId);
        //GL.DrawArrays(PrimitiveType.Triangles, 0, _mesh.Triangles.Length * 3);
        GL.BindVertexArray(0);
    }

    public void Dispose() {
        GL.DeleteVertexArray(_vertexArrayId);
        GL.DeleteBuffer(_vertexBufferId);
        GC.SuppressFinalize(this);
    }
}
