using System.Drawing;
using System.Numerics;
using System.Windows.Media;
using CornellBox.Models;
using CornellBox.Models.Objects;

namespace CornellBox.Scenes
{
    public static class CornellBoxScene
    {
        private static readonly Vector3 Eye = new Vector3(0, 0, -4);
        private static readonly Vector3 LookAt = new Vector3(0, 0, 6);

        private const float Fov = 36;

        private static readonly Material Mat1 = new Material(
            SceneManager.StoneTexture, Material.ProjectionMode.Spherical, emission: 1f);

        private static readonly Material Mat2 = new Material(
            SceneManager.WoodTexture,
            Material.ProjectionMode.Spherical,
            new Tiling(new Vector2(2f, 2f), Vector2.Zero),
            kreflection: 0.1f, //0.3
            specularK: 10,
            kdiffuse: 1.5f,
            kspecular: 0.5f,
            emission: 0.5f);

        private static readonly IObject[] Spheres = {
            new Sphere(new Vector3(-0.6f, 0.7f, -0.6f), 0.3f, Mat1),
            new Sphere(new Vector3(0.3f, 0.4f, 0.3f), 0.6f, Colors.LightCyan, Mat2),
            //new Triangle(new (Vector3 vert, Vector3 norm)[]
            //{
            //    (Vector3.Zero, -Vector3.UnitZ),
            //    (new Vector3(1,0,0), -Vector3.UnitZ),
            //    (new Vector3(0, -1f,0), -Vector3.UnitZ)
            //}, new Material(Colors.Red)), 
        };


        //03.JPG
        private static readonly HdrImage WallTexture = SceneManager.BrickTexture;
        private static readonly Material WallMat = new Material(WallTexture, Material.ProjectionMode.Planar);
        private static readonly HdrImage GroundTexture = SceneManager.GravelTexture;
        private static readonly Material GroundMat = new Material(GroundTexture, Material.ProjectionMode.Planar, new Tiling(Vector2.One * 0.6f, Vector2.Zero));

        private static readonly Models.Objects.Plane[] Planes =
        {
            new Models.Objects.Plane(new Vector3(1,0,0),  new Vector3(-1,0,0), WallMat),  //left
            new Models.Objects.Plane(new Vector3(-1,0,0), new Vector3(1,0,0), WallMat), //right
            new Models.Objects.Plane(new Vector3(0,0,-1), new Vector3(0,0,1), WallMat), //front
            new Models.Objects.Plane(new Vector3(0,0,1),  new Vector3(0,0,-4.1f), WallMat), //back
            new Models.Objects.Plane(new Vector3(0,-1,0), new Vector3(0,1,0), GroundMat),//bottom
            new Models.Objects.Plane(new Vector3(0,1,0),  new Vector3(0,-1,0),
                new Material(Colors.White)),//top
        };

        private static readonly Light[] Lights = {
            new Light(new Vector3(0,-0.98f, -0.8f), Colors.Yellow.ToVector3(), 1f),
            new Light(new Vector3(0.7f,-0.98f, 0.8f), Colors.Aqua.ToVector3(), 1f),
            new Light(new Vector3(-0.7f,-0.98f, 0.8f), Colors.BlueViolet.ToVector3(), 1f),
        };

        private static readonly Vector3 EnviromentLight = new Vector3(1f, 1f, 1f) * 0f;

        //private static readonly Mesh[] Meshes = new[]
        //{
        //    SceneManager.MonkeyMesh
        //};
        public static Scene Scene => new Scene(Spheres, Planes, Lights, EnviromentLight, Eye, LookAt, Fov);
    }
}
