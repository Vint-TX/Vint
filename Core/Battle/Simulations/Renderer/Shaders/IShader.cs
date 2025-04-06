using OpenTK.Mathematics;

namespace Vint.Core.Battle.Simulations.Renderer.Shaders;

public interface IShader {
    void Use();
    void SetMatrix4(string name, Matrix4 matrix);
    void SetVector3(string name, Vector3 vector);
}
