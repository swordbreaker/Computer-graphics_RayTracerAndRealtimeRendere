using System.Drawing;
using System.Numerics;
using System.Windows.Media;
using CornellBox.Models;
using Plane = CornellBox.Models.Objects.Plane;

namespace CornellBox.Scenes
{
    public class AntiAliasingScene
    {
        private static readonly Vector3 Eye = new Vector3(0, -0.3f, 0);
        private static readonly Vector3 LookAt = new Vector3(-1f, 0f, 1f);

        private static readonly HdrImage CheckerBoard = new HdrImage((Bitmap)Image.FromFile("Textures/checkerboard.jpg"));

        private const float Fov = 36;

        private static readonly IObject[] Objects = {};

        private static readonly Plane[] Planes =
        {
            new Plane(new Vector3(0,-1,0),  new Vector3(0,0.3f,0), new Material(CheckerBoard, Material.ProjectionMode.Planar))
        };

        private static readonly Light[] Lights = {
            new Light(new Vector3(-3f,-4f, -2f), Colors.White.ToVector3(), 0.5f),
            new Light(new Vector3(-1f,-4f, -4f), Colors.White.ToVector3(), 0.2f),
            new Light(new Vector3(2f,-4f, 3f), Colors.White.ToVector3(), 0.2f),
        };

        private static readonly Vector3 EnviromentLight = new Vector3(1f, 1f, 1f) * 0.5f;

        public static Scene Scene => new Scene(Objects, Planes, Lights, EnviromentLight, Eye, LookAt, Fov);
    }
}
