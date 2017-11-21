using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using CornellBox.Helpers;

namespace CornellBox.Models.Objects
{
    public class Sphere : IObject
    {
        private readonly Vector3 _center;
        private readonly float _radius;

        public Material Material => _material;

        private readonly Material _material;

        public Sphere(Vector3 center, float radius, Color color, Material? material = null) : this(center, radius, material)
        {
            _material.Color = color.ToVector3();
        }
        public Sphere(Vector3 center, float radius, Material? material = null)
        {
            _center = center;
            _radius = radius;
            _material = material ?? Material.DefaultMaterial;
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public (float lambda, IObject obj) ClosestIntersectionLambda(Ray ray)
        {
            var lambda = ClosestIntersectionLambda(ray, _center, _radius);

            return lambda >= float.MaxValue ? (lambda, null) : (lambda, this);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public static float ClosestIntersectionLambda(Ray ray, Vector3 center, float radius)
        {
            var ce = ray.StartPos - center;
            var d = ray.Direction;

            var b = Vector3.Dot(d, ce);
            var c = -ce.LengthSquared() + radius * radius;

            var b4ac = b * b + c;

            if (b4ac < 0) return float.MaxValue;

            var sqrt = Mathf.Sqrt(b4ac);

            var lambda1 = -b + sqrt;
            var lambda2 = -b - sqrt;

            if (lambda1 < 0)
            {
                if (lambda2 < 0) return float.MaxValue;
                return lambda2;
            }
            if (lambda2 < 0) return lambda1;

            return Math.Min(lambda1, lambda2);
        }

        public Vector3 GetNorm(Vector3 pos)
        {
            return (pos - _center) / _radius;
        }

        public Vector3 ToLocal(Vector3 pos)
        {
            return pos - _center;
        }

        public Vector3 ToLocalNormalized(Vector3 pos)
        {
            Contract.Assert(!float.IsNaN(pos.X), "!float.IsNaN(pos.X)");
            Contract.Assert(!float.IsNaN(pos.Y), "!float.IsNaN(pos.Y)");
            Contract.Assert(!float.IsNaN(pos.Z), "!float.IsNaN(pos.Z)");
            return Vector3.Normalize(pos - _center);
        }

        public BoundingSphere GetBounds()
        {
            return new BoundingSphere(_center, _radius);
        }

        public override string ToString()
        {
            return $"C: {_center} R: {_radius}";
        }
    }
}