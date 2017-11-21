using System;
using System.Numerics;
using System.Windows.Media;

namespace CornellBox.Models
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
            return new Vector3((float)c.R / 255, (float)c.G / 255, (float)c.B / 255);
        }
    }
}
