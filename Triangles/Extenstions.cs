using System;
using System.Numerics;
using System.Windows;
using System.Windows.Media;

namespace Triangles
{
    public static class Extenstions
    {
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            return val.CompareTo(max) > 0 ? max : val;
        }

        public static Color ToColor(this Vector3 v)
        {
            var x = v.X.Clamp(0f, 1f);
            var y = v.Y.Clamp(0f, 1f);
            var z = v.Z.Clamp(0f, 1f);
            return Color.FromArgb(255, (byte)(x * 255), (byte)(y * 255), (byte)(z * 255));
        }

        public static Vector3 ToVector3(this Color c)
        {
            return new Vector3(c.ScR, c.ScG, c.ScB);
        }

        public static Point ToPoint(this Vector3 v)
        {
            return new Point(v.X, v.Y);
        }

        public static Vector4 TransformNormalized(this Vector3 v, Matrix4x4 m)
        {
            var v4 = Vector4.Transform(v, Matrix4x4.Transpose(m));
            var w = v4.W;
            v4 /= v4.W;
            return new Vector4(v4.X, v4.Y, v4.Z, w);
        }

        public static Vector3 TransformNormalized3(this Vector3 v, Matrix4x4 m)
        {
            var v4 = Vector4.Transform(v, Matrix4x4.Transpose(m));
            v4 /= v4.W;
            return new Vector3(v4.X, v4.Y, v4.Z);
        }

        public static Vector3 ToVector3(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static void Clear<T>(this T[,] a, T value)
        {
            for (int x = 0; x < a.GetLength(0); x++)
            {
                for (int y = 0; y < a.GetLength(1); y++)
                {
                    a[x, y] = value;
                }
            }
        }

        public static void Clear<T>(this T[] a, T value)
        {
            for (int x = 0; x < a.Length; x++)
            {
                a[x] = value;
            }
        }
    }
}
