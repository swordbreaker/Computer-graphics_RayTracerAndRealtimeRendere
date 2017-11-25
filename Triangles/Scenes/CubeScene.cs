using System.Drawing;
using System.Numerics;
using System.Windows.Media;
using Triangles.Models;

namespace Triangles.Scenes
{
    public class CubeScene : IScene
    {
        public Mesh[] Meshes { get; }
        public Vector3 LightPos { get; } = new Vector3(1, 1, -5);
        public Vector3 EnviromentLight { get; } = new Vector3(1f, 1f, 1f) * 0.1f;

        private readonly (int a, int b, int c)[] _triangleIdx = {
            (0, 1, 2), //Top
            (0, 2, 3),
            (7, 6, 5), //bottom
            (7, 5, 4),
            (8, 9, 10), //left
            (8, 10, 11),
            (12, 13, 14), //right
            (14, 15, 12),
            (16, 18, 19), //front
            (16, 17, 18),
            (20, 21, 22), //back
            (22, 23, 20)
        };

        private readonly Vertex[] _verts = {
            //bottom
            new Vertex(new Vector3(-1, -1, -1), Colors.White.ToVector3(), -Vector3.UnitZ, new Vector2(0,1)), //0
            new Vertex(new Vector3(+1, -1, -1), Colors.Blue.ToVector3(), -Vector3.UnitZ, new Vector2(1,1)), //1
            new Vertex(new Vector3(+1, +1, -1), Colors.Red.ToVector3(), -Vector3.UnitZ, new Vector2(1,0)), //2
            new Vertex(new Vector3(-1, +1, -1), Colors.Violet.ToVector3(), -Vector3.UnitZ, new Vector2(0,0)), //3
            //Top
            new Vertex(new Vector3(-1, -1, +1), Colors.White.ToVector3(), Vector3.UnitZ, new Vector2(0,1)), //4
            new Vertex(new Vector3(+1, -1, +1), Colors.Green.ToVector3(), Vector3.UnitZ, new Vector2(1,1)), //5
            new Vertex(new Vector3(+1, +1, +1), Colors.Yellow.ToVector3(), Vector3.UnitZ, new Vector2(1,0)), //6
            new Vertex(new Vector3(-1, +1, +1), Colors.Orange.ToVector3(), Vector3.UnitZ, new Vector2(0,0)), //7
            //Left
            new Vertex(new Vector3(-1, -1, -1), Colors.Green.ToVector3(), -Vector3.UnitX, new Vector2(0,1)), //8
            new Vertex(new Vector3(-1, +1, -1), Colors.GreenYellow.ToVector3(), -Vector3.UnitX, new Vector2(1,1)), //9
            new Vertex(new Vector3(-1, +1, +1), Colors.LightGreen.ToVector3(), -Vector3.UnitX, new Vector2(1,0)), //10
            new Vertex(new Vector3(-1, -1, +1), Colors.PaleGreen.ToVector3(), -Vector3.UnitX, new Vector2(0,0)), //11
            //Right
            new Vertex(new Vector3(+1, -1, +1), Colors.LightYellow.ToVector3(), Vector3.UnitX, new Vector2(0,1)), //12
            new Vertex(new Vector3(+1, +1, +1), Colors.Yellow.ToVector3(), Vector3.UnitX, new Vector2(1,1)), //13
            new Vertex(new Vector3(+1, +1, -1), Colors.RosyBrown.ToVector3(), Vector3.UnitX, new Vector2(1,0)), //14
            new Vertex(new Vector3(+1, -1, -1), Colors.Aqua.ToVector3(), Vector3.UnitX, new Vector2(0,0)), //15
            //Front
            new Vertex(new Vector3(-1, +1, -1), Colors.Pink.ToVector3(), Vector3.UnitY, new Vector2(0,1)), //16
            new Vertex(new Vector3(+1, +1, -1), Colors.PaleTurquoise.ToVector3(), Vector3.UnitY, new Vector2(1,1)), //17
            new Vertex(new Vector3(+1, +1, +1), Colors.PowderBlue.ToVector3(), Vector3.UnitY, new Vector2(1,0)), //18
            new Vertex(new Vector3(-1, +1, +1), Colors.Peru.ToVector3(), Vector3.UnitY, new Vector2(0,0)), //19
            //Back
            new Vertex(new Vector3(-1, -1, +1), Colors.Orange.ToVector3(), -Vector3.UnitY, new Vector2(0,1)), //20
            new Vertex(new Vector3(+1, -1, +1), Colors.OrangeRed.ToVector3(), -Vector3.UnitY, new Vector2(1,1)), //21
            new Vertex(new Vector3(+1, -1, -1), Colors.DarkOrchid.ToVector3(), -Vector3.UnitY, new Vector2(1,0)), //22
            new Vertex(new Vector3(-1, -1, -1), Colors.OliveDrab.ToVector3(), -Vector3.UnitY, new Vector2(0,0)), //23
        };

        private float _alpha;

        private readonly Matrix4x4 _translation = Matrix4x4.CreateTranslation(0, 0, 0);

        public CubeScene()
        {
            var cube = new Mesh(_triangleIdx, _verts, new TextureMaterial((Bitmap)Image.FromFile(@"Textures/checkerboard.jpg"), TextureMaterial.TextureMode.Repeate));

            cube.UpdateAction += (mesh, deltaTime) =>
            {
                var roation = Matrix4x4.CreateFromYawPitchRoll(_alpha, _alpha, 0);
                mesh.M = roation * _translation;
                _alpha += 0.5f * deltaTime;
            };

            Meshes = new[] { cube };
        }
    }
}
