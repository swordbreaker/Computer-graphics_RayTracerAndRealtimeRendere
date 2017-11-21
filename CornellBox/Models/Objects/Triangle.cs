using System.Diagnostics.Contracts;
using System.Numerics;
using CornellBox.Helpers;

namespace CornellBox.Models.Objects
{
    /// <summary>
    /// Does not work with Acceleration Structures
    /// </summary>
    public class Triangle : IObject
    {
        private Vector3[] _verts = new Vector3[3];
        private Vector3[] _norms = new Vector3[3];

        private Vector3[] _edges = new Vector3[3];

        public Triangle((Vector3 vert, Vector3 norm)[] data, Material material)
        {
            Contract.Assert(data.Length == 3, "data.Length == 3");
            _verts[0] = data[0].vert;
            _verts[1] = data[1].vert;
            _verts[2] = data[2].vert;

            _norms[0] = data[0].norm;
            _norms[1] = data[1].norm;
            _norms[2] = data[2].norm;

            _edges[0] = _verts[1] - _verts[0];
            _edges[1] = _verts[2] - _verts[0];
            _edges[2] = _verts[2] - _verts[1];

            Material = material;
        }

        public Triangle(Vector3[] verts, Vector3[] norms, Material material)
        {
            Contract.Assert(verts.Length == 3, "verts.Length == 3");
            Contract.Assert(norms.Length == 3, "norms.Length == 3");

            _verts = verts;
            _norms = norms;

            _edges[0] = _verts[1] - _verts[0];
            _edges[1] = _verts[2] - _verts[0];
            _edges[2] = _verts[2] - _verts[1];

            Material = material;
        }

        public (float lambda, IObject obj) ClosestIntersectionLambda(Ray ray)
        {
            var h = Vector3.Cross(ray.Direction, _edges[1]);
            var a = Vector3.Dot(_edges[0], h);

            if (a > -Mathf.Epsilon && a < Mathf.Epsilon) return (float.MaxValue, null);

            var f = 1 / a;
            var s = ray.StartPos - _verts[0];

            var u = f * Vector3.Dot(s, h);
            if (u < 0.0 || u > 1.0) return (float.MaxValue, null);

            var q = Vector3.Cross(s, _edges[0]);
            var v = f * Vector3.Dot(ray.Direction, q);
            if (v < 0 || u + v > 1) return (float.MaxValue, null);

            // At this stage we can compute t to find out where the intersection point is on the line.
            var t = f * Vector3.Dot(_edges[1], q);
            return t > Mathf.Epsilon ? (t, this) : (float.MaxValue, null);
        }

        public BoundingSphere GetBounds()
        {
            // Calculate relative distances

            var A = Vector3.Distance(_verts[0], _verts[1]);
            var B = Vector3.Distance(_verts[1], _verts[2]);
            var C = Vector3.Distance(_verts[2], _verts[0]);

            var a = _verts[2];
            var b = _verts[0];
            var c = _verts[1];

            if (B < C)
            {
                (B, C) = (C, B);
                (b, c) = (c, b);
            }
            if (A < B)
            {
                (A, B) = (B, A);
                (a, b) = (b, a);
            }

            // If obtuse, just use longest diameter, otherwise circumscribe
            if ((B * B) + (C * C) <= (A * A))
            {
                return new BoundingSphere((b + c) / 2f, A/2f);
            }
            else
            {
                // http://en.wikipedia.org/wiki/Circumscribed_circle
                var cosA = (B * B + C * C - A * A) / (B * C * 2);
                var r = A / Mathf.Sqrt(1 - cosA * cosA) * 2f;
                var alpha = a - c;
                var beta = b - c;


                var x = beta * Vector3.Dot(alpha, alpha) - alpha * Vector3.Dot(beta, beta);
                var y = Vector3.Cross(x, Vector3.Cross(alpha, beta));
                var z = Vector3.Dot(Vector3.Cross(alpha, beta), Vector3.Cross(alpha, beta)) * 2f;
                var pos = y / z + c;

                return new BoundingSphere(pos, r);
                ////s.radius = A / (sqrt(1 - cos_a * cos_a) * 2.f);
                //Vec3 alpha = *a - *c, beta = *b - *c;
                //s.position = (beta * alpha.dot(alpha) - alpha * beta.dot(beta)).cross(alpha.cross(beta)) /
                //             (alpha.cross(beta).dot(alpha.cross(beta)) * 2.f) + *c;
            }
        }

        public Material Material { get; }
        public Vector3 GetNorm(Vector3 pos)
        {
            return _norms[0];
        }

        public Vector3 ToLocal(Vector3 pos)
        {
            return pos - _verts[0];
        }

        public Vector3 ToLocalNormalized(Vector3 pos)
        {
            return Vector3.Normalize(pos - _verts[0]);
        }
    }
}
