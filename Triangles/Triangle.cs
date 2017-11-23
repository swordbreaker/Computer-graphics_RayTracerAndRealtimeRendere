using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Triangles.Models;

namespace Triangles
{
    public struct Triangle
    {
        public readonly Vector3 A;
        public readonly Vector3 B;
        public readonly Vector3 C;
        public readonly Vector3 AB;
        public readonly Vector3 AC;
        public readonly Vector3 Normal;

        public Vector3 AColor => VertColors[0];
        public Vector3 BColor => VertColors[1];
        public Vector3 CColor => VertColors[2];

        public Vector3 ANormal => VertNormals[0];
        public Vector3 BNormal => VertNormals[1];
        public Vector3 CNormal => VertNormals[2];

        public readonly Vector4[] Verts;
        public readonly Vector3[] VertColors;
        public readonly Vector3[] VertNormals;
        public readonly Vector2[] VertUvs;

        public IMaterial Material;

        public bool IsClockWise => Normal.Z <= 0;
        public readonly Vector3 Center;

        public Vector3 Min
        {
            get
            {
                var min = Vector3.Min(A, B);
                return Vector3.Min(min, C);
            }
        }

        public Vector3 Max
        {
            get
            {
                var min = Vector3.Max(A, B);
                return Vector3.Max(min, C);
            }
        }

        public Triangle(Vector3 a, Vector3 b, Vector3 c, Vector3[] colors, Vector3[] normals, Vector2[] uvs, IMaterial material)
        {
            Contract.Assert(colors.Length == 3, "colors.Length == 3");
            Contract.Assert(normals.Length == 3, "colors.Length == 3");

            A = a;
            B = b;
            C = c;
            AB = B - A;
            AC = C - A;
            Normal = Vector3.Normalize(Vector3.Cross(AB, AC));
            Center = (A + B + C) / 3;

            VertColors = colors;
            VertNormals = normals;
            VertUvs = uvs;
            Verts = new[] {new Vector4(a, 0), new Vector4(b, 0), new Vector4(c, 0), };

            Material = material;
        }

        public Triangle(Vector4 a, Vector4 b, Vector4 c, Vector3[] colors, Vector3[] normals, Vector2[] uvs, IMaterial material)
        {
            Contract.Assert(colors.Length == 3, "colors.Length == 3");
            Contract.Assert(normals.Length == 3, "colors.Length == 3");

            A = a.ToVector3();
            B = b.ToVector3();
            C = c.ToVector3();
            AB = B - A;
            AC = C - A;
            Normal = Vector3.Normalize(Vector3.Cross(AB, AC));
            Center = (A + B + C) / 3;

            VertColors = colors;
            VertNormals = normals;
            Verts = new[] { a, b, c, };
            VertUvs = uvs;
            Material = material;
        }

        public Triangle(Vector3 a, Vector3 b, Vector3 c, Color[] colors, Vector3[] normals, Vector2[] uvs, IMaterial material) : this(a, b, c, colors.Select(co => co.ToVector3()).ToArray(),normals, uvs, material) { }
        public Triangle(Vector3 a, Vector3 b, Vector3 c, Color color, Vector3[] normals, Vector2[] uvs, IMaterial material) : this(a, b, c, new [] {color, color, color}, normals, uvs, material) { }
        public Triangle(Triangle t, Vector3[] colors, Vector3[] normals,Vector2[] uvs, IMaterial material) : this(t.A, t.B, t.C, colors, normals, uvs, material) { }

        public Triangle Transform(Matrix4x4 m)
        {
            return new Triangle(
                Vector3.Transform(A, m),
                Vector3.Transform(B, m),
                Vector3.Transform(C, m),
                VertColors,
                VertNormals,
                VertUvs,
                Material
            );
        }

        public Triangle TransformNormalized(Matrix4x4 m)
        {
            return new Triangle(
                A.TransformNormalized(m),
                B.TransformNormalized(m),
                C.TransformNormalized(m),
                VertColors,
                VertNormals.Select(n => Vector3.Normalize(n.TransformNormalized3(m))).ToArray(),
                VertUvs,
                Material
            );
        }

        public Triangle SetLambert(Vector3 light)
        {
            var l = Vector3.Normalize(light - Center);
            
            VertColors[0] = VertColors[0] * Math.Max(0f, Vector3.Dot(Normal, l));
            return this;
        }

        public Polygon ToPolygon()
        {
            var poly = new Polygon() {Stroke = Brushes.Black, Fill = new SolidColorBrush(VertColors[0].ToColor())};
            poly.Points.Add(A.ToPoint());
            poly.Points.Add(B.ToPoint());
            poly.Points.Add(C.ToPoint());
            return poly;
        }

        /// <summary>
        /// [Ax, Ay, Az
        /// Bx, By, Bz
        /// Cx, Cy, Cz
        /// </summary>
        /// <returns></returns>

        public float[] ToArray()
        {
            return new float[]
            {
                Verts[0].X, Verts[0].Y, Verts[0].Z, Verts[0].W,
                Verts[1].X, Verts[1].Y, Verts[1].Z, Verts[1].W,
                Verts[2].X, Verts[2].Y, Verts[2].Z, Verts[2].W,
                VertNormals[0].X, VertNormals[0].Y, VertNormals[0].Z,
                VertNormals[1].X, VertNormals[1].Y, VertNormals[1].Z,
                VertNormals[2].X, VertNormals[2].Y, VertNormals[2].Z,
                VertUvs[0].X, VertUvs[0].Y,
                VertUvs[1].X, VertUvs[1].Y,
                VertUvs[2].X, VertUvs[2].Y,
                VertColors[0].X, VertColors[0].Y, VertColors[0].Z,
                VertColors[1].X, VertColors[1].Y, VertColors[1].Z,
                VertColors[2].X, VertColors[2].Y, VertColors[2].Z,
            };
        }
    }
}
