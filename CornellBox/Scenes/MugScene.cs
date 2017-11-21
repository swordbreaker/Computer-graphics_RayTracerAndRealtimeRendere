using System;
using System.Numerics;
using System.Windows.Media;
using CornellBox.Helpers;
using CornellBox.Models;
using CornellBox.Models.Objects;

namespace CornellBox.Scenes
{
    public class MugScene
    {
        private static readonly Vector3 Eye = new Vector3(0.5f, -1.5f, 1.5f);
        private static readonly Vector3 LookAt = new Vector3(0, 0, -0.2f);

        private const float Fov = 40;

        private static readonly Material Mat1 = new Material(
            SceneManager.StoneTexture, Material.ProjectionMode.Spherical, emission: 1f);

        private static IObject[] Spheres = {
        };

        private static readonly Models.Objects.Plane[] Planes =
        {
            new Models.Objects.Plane(new Vector3(0,-1,0), new Vector3(0,0,0)),//bottom
        };

        private static Light[] Lights = {
            new Light(new Vector3(-1,-1.5f, 0f), Colors.White.ToVector3(), 0.5f),
            new Light(new Vector3(1f,-1.5f, 0f), Colors.White.ToVector3(), 0.5f),
            new Light(new Vector3(0f,-1.5f, 1f), Colors.White.ToVector3(), 0.5f),
            new Light(new Vector3(0f,-1.5f, -1f), Colors.White.ToVector3(), 0.5f),
        };

        private static readonly Vector3 EnviromentLight = new Vector3(1f, 1f, 1f) * 0.1f;

        private static readonly Mesh[] Meshes = new[]
        {
            SceneManager.MugMesh, 
        };

        private const int _lights = 30;
        private const float _radius = 1.7f;
        private const float _height = -1.5f;

        public static Scene Scene
        {
            get
            {
                Lights = new Light[_lights];
                //Spheres = new IObject[_lights];
                for (int i = 0; i < _lights; i++)
                {
                    var pos = new Vector3((float)Math.Cos(i) * _radius, _height, (float)Math.Sin(i) * _radius);
                    Lights[i] = new Light(pos, Colors.White.ToVector3(), 0.05f);

                    //Spheres[i] = new Sphere(pos, 0.1f, new Material(Colors.White, emission: 1f));
                }


                return new Scene(Spheres, Planes, Lights, EnviromentLight, Eye, LookAt, Fov, Meshes);
            }
        }
    }
}
