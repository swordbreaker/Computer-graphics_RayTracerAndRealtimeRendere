using System.Numerics;
using System.Windows.Media;
using CornellBox.Models;
using CornellBox.Models.Objects;

namespace CornellBox.Scenes
{
    public class SkyBoxScene
    {
        private static readonly Vector3 Eye = new Vector3(-1, -2, -4);
        private static readonly Vector3 LookAt = new Vector3(0f, 0f, 0f);

        private const float Fov = 36;

        private static readonly Material Mat1 = new Material(
            Colors.White, 
            kreflection: 0.2f,
            specularK: 50
            );

        private static readonly IObject[] Objects = {
            new Sphere(new Vector3(0.6f, -0.3f, 0f), 0.6f, Mat1),
            new Sphere(new Vector3(-0.8f, -0.15f, -1f), 0.3f, Colors.Black, Mat1),
            new LightProbe(Vector3.Zero, 10f, SceneManager.GraceProbeTextuere),
        };

        private static readonly Models.Objects.Plane[] Planes =
        {
            new Models.Objects.Plane(new Vector3(0,-1,0),  new Vector3(0,0.3f,0), new Material(Colors.White, 0.1f, 20))
        };

        private static readonly Light[] Lights = {
            new Light(new Vector3(-3f,-4f, -2f), Colors.White.ToVector3(), 0.5f),
            new Light(new Vector3(-1f,-4f, -4f), Colors.White.ToVector3(), 0.2f),
            new Light(new Vector3(2f,-4f, 3f), Colors.White.ToVector3(), 0.2f),
        };

        private static readonly Vector3 EnviromentLight = new Vector3(1f, 1f, 1f) * 0f;

        public static Scene Scene => new Scene(Objects, Planes, Lights, EnviromentLight, Eye, LookAt, Fov);
    }
}
