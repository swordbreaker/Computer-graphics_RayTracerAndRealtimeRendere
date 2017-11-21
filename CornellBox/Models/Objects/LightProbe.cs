using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace CornellBox.Models.Objects
{
    public class LightProbe : IObject
    {
        private readonly Vector3 _center;
        private readonly float _radius;

        public Material Material { get; }

        public LightProbe(Vector3 pos, float radius, HdrImage image)
        {
            _center = pos;
            _radius = radius;
            Material = new Material(image, Material.ProjectionMode.LightProbe, new Tiling(), 0, 0, 0,0, 1f);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public (float lambda, IObject obj) ClosestIntersectionLambda(Ray ray)
        {
            var lambda = Sphere.ClosestIntersectionLambda(ray, _center, _radius);

            return lambda >= float.MaxValue ? (lambda, null) : (lambda, this);
        }

        public BoundingSphere GetBounds()
        {
            return new BoundingSphere(_center, _radius);
        }

        public Vector3 GetNorm(Vector3 pos)
        {
            return (_center - pos) / _radius;
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
    }
}
