using System;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Windows.Media;
using CornellBox.Helpers;
using Color = System.Windows.Media.Color;

namespace CornellBox.Models
{
    public struct Material
    {
        public enum ProjectionMode
        {
            SolidColor,
            Planar,
            Spherical,
            Qubic,
            LightProbe
        }

        public static Material DefaultMaterial => new Material(Colors.White);

        public int SpecularK { get; set; }
        public float Kreflection { get; set; }
        public float Kspecular { get; set; }
        public float Kdiffuse { get; set; }
        public Vector3 Color { get; set; }
        public HdrImage HdrImage { get; set; }
        public Tiling Tiling { get; set; }
        public float Emission { get; set; }

        public ProjectionMode UvProjection { get; set; }

        public Material(Color color, float kreflection = 0, int specularK = 10, float kdiffuse = 2f, float kspecular = 1f, float emission = 0f) :this(kreflection, specularK, kdiffuse, kspecular, emission)
        {
            UvProjection = ProjectionMode.SolidColor;
            Color = color.ToVector3();
        }

        public Material(HdrImage bitmap, ProjectionMode projectionMode, Tiling tiling = null, float kreflection = 0, int specularK = 10, float kdiffuse = 2f, float kspecular = 0.5f, float emission = 0f) : this(kreflection, specularK, kdiffuse, kspecular, emission)
        {
            UvProjection = projectionMode;
            HdrImage = bitmap;
            Tiling = tiling ?? new Tiling();
        }

        public Material(float kreflection = 0, int specularK = 10, float kdiffuse = 2f, float kspecular = 0.5f, float emission = 0f)
        {
            UvProjection = ProjectionMode.SolidColor;
            Color = Colors.White.ToVector3();
            SpecularK = specularK;
            Kreflection = kreflection;
            Kdiffuse = kdiffuse;
            Kspecular = kspecular;
            Tiling = new Tiling();
            HdrImage = null;
            Emission = emission;
        }

        public Vector3 GetColor(Vector3 v)
        {
            switch (UvProjection)
            {
                case ProjectionMode.SolidColor:
                    Contract.Assert(Color != null, "Color != null");
                    return Color;
                case ProjectionMode.Planar:
                    Contract.Assert(HdrImage != null, "Bitmap != null");
                    return GetPlanarColor(v);
                case ProjectionMode.Spherical:
                    Contract.Assert(HdrImage != null, "Bitmap != null");
                    return GetSphericalColor(v);
                case ProjectionMode.LightProbe:
                    return GetLightProbeColor(v);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Vector3 GetSphericalColor(Vector3 vec)
        {
            DebugHelper.CheckRange(vec.X, -1, 1, $"vecX: {vec.X} not in range (-1, 1)");
            DebugHelper.CheckRange(vec.Y, -1, 1, $"vecY: {vec.Y} not in range (-1, 1)");
            DebugHelper.CheckRange(vec.Z, -1, 1, $"vecZ: {vec.Z} not in range (-1, 1)");

            var u = Mathf.Atan2(vec.X, vec.Z) / (2f *Mathf.PI);
            var v = Mathf.Acos(vec.Y) / (2F * Mathf.PI);

            u = (u * Tiling.Size.X + Tiling.Offset.X) % 1;
            v = (v * Tiling.Size.Y + Tiling.Offset.Y) % 1;

            return GetPixelColor(u, v);
        }

        private Vector3 GetPlanarColor(Vector3 vec)
        {
            DebugHelper.CheckRange(vec.X, -1, 1, $"vecX: {vec.X} not in range (-1, 1)");
            DebugHelper.CheckRange(vec.Y, -1, 1, $"vecY: {vec.Y} not in range (-1, 1)");
            DebugHelper.CheckRange(vec.Z, -1, 1, $"vecZ: {vec.Z} not in range (-1, 1)");
            
            var u = vec.X;
            var v = vec.Y;

            return GetPixelColor(u, v);
        }

        private Vector3 GetLightProbeColor(Vector3 vec)
        {
            DebugHelper.CheckRange(vec.X, -1, 1, $"vecX: {vec.X} not in range (-1, 1)");
            DebugHelper.CheckRange(vec.Y, -1, 1, $"vecY: {vec.Y} not in range (-1, 1)");
            DebugHelper.CheckRange(vec.Z, -1, 1, $"vecZ: {vec.Z} not in range (-1, 1)");

            var r = (1 / Mathf.PI) * Mathf.Acos(vec.Z) / Mathf.Sqrt(vec.X * vec.X + vec.Y * vec.Y);

            var u = vec.X * r * -1;
            var v = vec.Y * r * -1;
            
            return GetPixelColor(u, v);
        }

        private Vector3 GetPixelColor(float u, float v)
        {
            DebugHelper.CheckRange(u, -1.0000001f, 1.0000001f, $"u: {u} not in range(-1, 1)");
            DebugHelper.CheckRange(v, -1.0000001f, 1.0000001f, $"u: {v} not in range(-1, 1)");

            (var w, var h) = (HdrImage.Width, HdrImage.Height);

            var x = (int)Mathf.Round(u * (w-1)/2 + (w-1)/2);
            var y = (int)Mathf.Round(v * (h-1)/2 + (h-1)/2);

            DebugHelper.CheckRange(x, 0, w, $"x not in range(0, {w})");
            DebugHelper.CheckRange(y, 0, h, $"y not in range(0, {h})");

            x = (int)((x * Tiling.Size.X + Tiling.Offset.X) % (w-1));
            y = (int)((y * Tiling.Size.Y + Tiling.Offset.Y) % (h-1));

            return HdrImage[x, y];
        }
    }

    public class Tiling
    {
        public Vector2 Offset;
        public Vector2 Size;

        public Tiling() : this(Vector2.One, Vector2.Zero) { }

        public Tiling(Vector2 size, Vector2 offset)
        {
            Offset = offset;
            Size = size;
        }
    }
}
