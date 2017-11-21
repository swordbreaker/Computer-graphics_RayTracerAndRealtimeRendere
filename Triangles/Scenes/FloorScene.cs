using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Triangles.Models;

namespace Triangles.Scenes
{
    public class FloorScene : IScene
    {
        public Mesh[] Meshes { get; }
        public Vector3 LightPos { get; } = new Vector3(0,0,-5);
        public Vector3 EnviromentLight { get; } = Vector3.One * 0.1f; 

        private readonly (int a, int b, int c)[] _triangleIdx = {
            (0, 1, 2),
            (2, 3, 0),
        };

        private readonly Vertex[] _verts = {
            new Vertex(new Vector3(-100, 0, +100), Colors.White.ToVector3(), Vector3.UnitY, new Vector2(0,1)),
            new Vertex(new Vector3(+100, 0, +100), Colors.White.ToVector3(), Vector3.UnitY, new Vector2(1,1)),
            new Vertex(new Vector3(+100, 0, -100), Colors.White.ToVector3(), Vector3.UnitY, new Vector2(1,0)),
            new Vertex(new Vector3(-100, 0, -100), Colors.White.ToVector3(), Vector3.UnitY, new Vector2(0,0)),
        };

        public FloorScene()
        {
            //var cube = new Mesh(_triangleIdx, _verts, new TextureMaterial((Bitmap)Image.FromFile(@"Textures/checkerboard.jpg"), TextureMaterial.TextureMode.Repeate));
            var cube = new Mesh(_triangleIdx, _verts, new ColorMaterial(Colors.Red));
            cube.M = Matrix4x4.CreateTranslation(0,-5,0);

            Meshes = new[] { cube };
        }
    }
}
