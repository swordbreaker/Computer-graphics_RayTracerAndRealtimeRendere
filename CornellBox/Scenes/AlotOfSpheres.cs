using System;
using System.Numerics;
using System.Windows.Media;
using CornellBox.Models;
using CornellBox.Models.Objects;

namespace CornellBox.Scenes
{
    public class AlotOfSpheres
    {
        private const int SphereCount = 1024;
        private static readonly Vector3 Eye = new Vector3(0, 3, -4);
        private static readonly Vector3 LookAt = new Vector3(0, -2, 6);

        private const float Fov = 36;

        private static readonly Light[] Lights = {
            new Light(Eye, Colors.White.ToVector3(), 0.6f),
        };

        private static readonly Vector3 EnviromentLight = new Vector3(1f, 1f, 1f) * 0f;

        private static readonly Random _random = new Random();

        public static Scene Scene
        {
            get
            {
                var spheres = new Sphere[SphereCount];

                for (int i = 0; i < spheres.Length; i++)
                {
                    var x = (float)_random.NextDouble() * 4f - 2f;
                    var y = (float)_random.NextDouble() * 4f - 2f;
                    var z = (float) _random.NextDouble() + 1;
                    var radius = (float)(_random.NextDouble() + 0.5) / 8;

                    var r = (byte)_random.Next(0, 255);
                    var g = (byte)_random.Next(0, 255);
                    var b = (byte)_random.Next(0, 255);

                    var mat = new Material(Color.FromRgb(r, g, b));

                    spheres[i] = new Sphere(new Vector3(x, y, z), radius, Color.FromRgb(r, g, b), mat);
                }

                return new Scene(spheres, new Models.Objects.Plane[0], Lights, EnviromentLight, Eye, LookAt, Fov);
            }
        }
    }
}