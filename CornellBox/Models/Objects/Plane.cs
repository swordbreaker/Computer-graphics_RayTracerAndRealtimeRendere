using System;
using System.Numerics;
using CornellBox.Helpers;

namespace CornellBox.Models.Objects
{
    public class Plane : IObject
    {
        public Material Material { get; }
        public Vector3 Center { get; set; }
        public Vector3 Normal { get; set; }
        public float D0 { get; set; }

        public Plane(Vector3 normal, Vector3 center, Material? material = null)
        {
            Material = material ?? Material.DefaultMaterial;
            Center = center;
            Normal = Vector3.Normalize(normal);
            D0 = Vector3.Dot(center, normal);
        }

        public Plane(Vector3 normal, Vector3 center, float d0)
        {
            Center = center;
            Normal = normal;
            D0 = d0;
        }

        public Plane(System.Numerics.Plane plane, Vector3 center) : this(plane.Normal, center, plane.D) { }

        public (float lambda, IObject obj) ClosestIntersectionLambda(Ray ray)
        {
            var dn = Vector3.Dot(ray.Direction, Normal);
            if(Math.Abs(dn) < Mathf.Epsilon) return (float.MaxValue, null);

            var lambda = (D0 - Vector3.Dot(ray.StartPos, Normal)) / Vector3.Dot(ray.Direction, Normal);

            return lambda < 0 ? (float.MaxValue, null) : (lambda, this);
        }

        public BoundingSphere GetBounds()
        {
            throw new NotImplementedException();
        }

        public Vector3 GetNorm(Vector3 pos)
        {
            return Normal;
        }

        public Vector3 ToLocal(Vector3 pos)
        {
            return pos - Center;
        }

        public Vector3 ToLocalNormalized(Vector3 pos)
        {
            var v = pos - Center;

            if (Math.Abs(Vector3.Dot(Vector3.UnitY, Normal) - 1) < Mathf.Epsilon)
            {
                return new Vector3(v.X % 1f, v.Z % 1f, 0);
            }

            if (Math.Abs(Vector3.Dot(Vector3.UnitY, Normal) + 1) < Mathf.Epsilon)
            {
                return new Vector3(-v.X % 1f, -v.Z % 1f, 0);
            }

            var r = Vector3.Normalize(Vector3.Cross(-Vector3.UnitY, Normal));
            var u = Vector3.Normalize(Vector3.Cross(r, Normal));

            var x = Vector3.Dot(v, r);
            var y = Vector3.Dot(v, u);

            return new Vector3(x % 1f, y % 1f, 0);
        }
    }
}
