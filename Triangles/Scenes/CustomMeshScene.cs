using System.Numerics;
using System.Windows.Media;
using Triangles.Models;

namespace Triangles.Scenes
{
    public class CustomMeshScene : IScene
    {
        public Mesh[] Meshes { get; }
        public Vector3 LightPos { get; } = new Vector3(0, 0, -5);
        public Vector3 EnviromentLight { get; } = new Vector3(1f, 1f, 1f) * 0.1f;

        public CustomMeshScene(string path)
        {
            Meshes = new[] {new Mesh(path, new ColorMaterial(Colors.White))};
        }
    }
}
