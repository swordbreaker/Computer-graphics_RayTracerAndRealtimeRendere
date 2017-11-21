using System.Numerics;
using System.Windows.Media;
using Triangles.Models;

namespace Triangles.Scenes
{
    public class MonkeyScene : IScene
    {
        public Mesh[] Meshes { get; } =
        {
            new Mesh(@"Meshes\monkey.obj", new ColorMaterial(Colors.White))
        };

        public Vector3 LightPos { get; } = new Vector3(0,0,-5);
        public Vector3 EnviromentLight { get; } = Vector3.Zero;
    }
}
