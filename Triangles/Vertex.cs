using System.Numerics;
using Triangles.Models;

namespace Triangles
{
    public struct Vertex
    {
        public Vector3 Pos;
        public Vector3 Color;
        public Vector3 Normal;
        public Vector2 Uv;

        public Vertex(Vector3 pos, Vector3 color, Vector3 normal, Vector2 uv)
        {
            Pos = pos;
            Color = color;
            Normal = normal;
            Uv = uv;
        }

        public Vertex Transform(Matrix4x4 m)
        {
            Matrix4x4.Invert(m, out var mn);
            mn = Matrix4x4.Transpose(mn);
            Pos = Vector3.Transform(Pos, m);
            Normal = Vector3.TransformNormal(Normal, mn);
            return this;
        }
    }
}
