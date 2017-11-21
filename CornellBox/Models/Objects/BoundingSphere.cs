using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace CornellBox.Models.Objects
{
    public class BoundingSphere : IRayTarget
    {
        public readonly Vector3 Center;
        public readonly float Radius;

        public IRayTarget Left { get; }
        public IRayTarget Right { get; }

        public BoundingSphere(IRayTarget left, IRayTarget right)
        {
            Left = left;
            Right = right;

            var bound1 = left.GetBounds();
            var bound2 = right.GetBounds();

            var c1c2 = bound2.Center - bound1.Center;
            var c1c2Lengt = c1c2.Length();


            //bound1 sphere is completely in bound2 sphere
            if (c1c2Lengt + bound1.Radius < bound2.Radius)
            {
                Center = bound2.Center;
                Radius = bound2.Radius;
            }
            //boud2 sphere is completely in bound1 sphere
            else if (c1c2Lengt + bound2.Radius < bound1.Radius)
            {
                Center = bound1.Center;
                Radius = bound1.Radius;
            }
            else
            {
                Radius = (c1c2Lengt + bound1.Radius + bound2.Radius) / 2;
                Center = bound1.Center + Vector3.Normalize(c1c2) * Math.Abs(Radius - bound1.Radius);
            }
        }

        public BoundingSphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public (float lambda, IObject obj) ClosestIntersectionLambda(Ray ray)
        {
            if (!IsIntersecting(ray)) return (float.MaxValue, null);
            
            var (l, lobj) = Left?.ClosestIntersectionLambda(ray) ?? (float.MaxValue, null);
            var (r, robj) = Right?.ClosestIntersectionLambda(ray) ?? (float.MaxValue, null);

            return l.CompareTo(r) < 1 ? (l, lobj) : (r, robj);
        }

        public bool IsIntersecting(Ray ray)
        {
            var ce = ray.StartPos - Center;
            var d = ray.Direction;

            var b = Vector3.Dot(d, ce);
            var c = -ce.LengthSquared() + Radius * Radius;

            var b4ac = b * b + c;

            if (b4ac < 0) return false;

            var lambda1 = -b + (float)Math.Sqrt(b4ac);
            var lambda2 = -b - (float)Math.Sqrt(b4ac);

            return lambda1 > 0 || lambda2 > 0;
        }

        public BoundingSphere GetBounds()
        {
            return this;
        }
    }
}
