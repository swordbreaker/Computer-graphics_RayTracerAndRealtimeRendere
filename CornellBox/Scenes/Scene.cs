using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CornellBox.Helpers;
using CornellBox.Models;
using CornellBox.Models.Objects;
using Plane = CornellBox.Models.Objects.Plane;

namespace CornellBox.Scenes
{
    public class Scene
    {
        public readonly HashSet<IRayTarget> Objects;
        public readonly Plane[] Planes;
        public readonly Light[] Lights;
        public readonly Vector3 EnviromentLight;
        public readonly Vector3 Eye;
        public Vector3 LookAt;
        public readonly float Fov;
        public readonly double Alpha;
        public readonly Mesh[] Meshes;

        public bool UseAccelerationStructures { get; set; } = true;
        
        public Scene(IObject[] objects, Plane[] planes, Light[] lights, Vector3 enviromentLight, Vector3 eye, Vector3 lookAt, float fov, Mesh[] meshes = null)
        {
            Objects = new HashSet<IRayTarget>(objects);
            Planes = planes;
            Lights = lights;
            EnviromentLight = enviromentLight;
            Eye = eye;
            LookAt = lookAt;
            Fov = fov;
            Alpha = Mathf.ToRadian(Fov);
            Meshes = meshes;
        }

        public Scene(Scene scene)
        {
            Objects = new HashSet<IRayTarget>(scene.Objects);
            Planes = scene.Planes;
            Lights = scene.Lights;
            EnviromentLight = scene.EnviromentLight;
            Eye = scene.Eye;
            LookAt = scene.LookAt;
            Fov = scene.Fov;
            Alpha = scene.Alpha;
            UseAccelerationStructures = scene.UseAccelerationStructures;
            Meshes = scene.Meshes;
        }

        public void Init()
        {
            if (Meshes != null)
            {
                foreach (var mesh in Meshes)
                {
                    foreach (var meshTriangle in mesh.Triangles)
                    {
                        Objects.Add(meshTriangle);
                    }
                }
            }

            if(UseAccelerationStructures) Sweep();
        }

        private void Sweep()
        {
            var comparer = Comparer<IRayTarget>.Create((o1, o2) =>
            {
                var cmp = o1.GetBounds().Center.X.CompareTo(o2.GetBounds().Center.X);
                return cmp != 0 ? cmp : o1.GetBounds().Center.Y.CompareTo(o2.GetBounds().Center.Y);
            });

            while (Objects.Count > 1)
            {
                var points = Objects.OrderBy(target => target, comparer).ToList();
                var l = new HashSet<IRayTarget> { points[0], points[1] };

                var min = Vector3.DistanceSquared(points[0].GetBounds().Center, points[1].GetBounds().Center);
                var minPairs = new ValueTuple<IRayTarget, IRayTarget>(points[0], points[1]);

                var left = 0;
                var right = 2;

                while (right < points.Count)
                {
                    if (points[left].GetBounds().Center.X <= points[right].GetBounds().Center.X - min)
                    {
                        l.Remove(points[left]);
                        left++;
                    }
                    else
                    {
                        l.Add(points[right]);
                        right++;
                        if (right < points.Count)
                        {
                            var p = l.OrderBy(target => Vector3.DistanceSquared(target.GetBounds().Center, points[right].GetBounds().Center)).First();
                            var dist = Vector3.DistanceSquared(p.GetBounds().Center, points[right].GetBounds().Center);
                            if (dist < min)
                            {
                                min = dist;
                                minPairs = (p, points[right]);
                            }
                        }
                    }
                }

                Objects.Remove(minPairs.Item1);
                Objects.Remove(minPairs.Item2);
                Objects.Add(new BoundingSphere(minPairs.Item1, minPairs.Item2));
            }
        }

        public void CreateTree()
        {
            var comparer = Comparer<IRayTarget>.Create((o1, o2) =>
            {
                var cmp = o1.GetBounds().Center.X.CompareTo(o2.GetBounds().Center.X);
                return cmp != 0 ? cmp : o1.GetBounds().Center.Y.CompareTo(o2.GetBounds().Center.Y);
            });

            while (Objects.Count > 1)
            {



                var minPairs = new ValueTuple<IRayTarget, IRayTarget>(null, null);
                var min = float.MaxValue;

                using (var enum1 = Objects.GetEnumerator())
                {
                    for (var i = 0; i < Objects.Count; i++)
                    {
                        enum1.MoveNext();
                        using (var enum2 = Objects.GetEnumerator())
                        {
                            enum2.MoveNext();

                            for (var k = 0; k < i; k++)
                            {
                                var dist = Vector3.DistanceSquared(enum1.Current.GetBounds().Center,
                                    enum2.Current.GetBounds().Center);
                                if (dist < min)
                                {
                                    min = dist;
                                    minPairs = (enum1.Current, enum2.Current);
                                }
                            }
                        }
                    }
                }

                Objects.Remove(minPairs.Item1);
                Objects.Remove(minPairs.Item2);
                Objects.Add(new BoundingSphere(minPairs.Item1, minPairs.Item2));
            }
        }
    }
}