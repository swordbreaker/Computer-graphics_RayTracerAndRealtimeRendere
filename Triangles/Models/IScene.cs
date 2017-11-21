using System.Numerics;

namespace Triangles.Models
{
    public interface IScene
    {
        Mesh[] Meshes { get; }
        Vector3 LightPos { get; }
        Vector3 EnviromentLight { get; }
    }
}
