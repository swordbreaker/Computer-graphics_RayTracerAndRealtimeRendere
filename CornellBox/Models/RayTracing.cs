using System;
using System.Numerics;
using System.Windows.Media;
using CornellBox.Helpers;
using CornellBox.Scenes;

namespace CornellBox.Models
{
    public class RayTracing
    {
        private readonly Scene _scene;
        private readonly int _imgWidth;
        private readonly int _imgHeight;

        public bool UseDiffues { get; set; } = true;
        public bool UseSpecular { get; set; } = true;
        public bool UseReflection { get; set; } = true;
        public bool UseShadows { get; set; } = true;
        public float Aperture { get; set; } = 0f;
        public bool UseGaussAntiAliasing { get; set; } = false;
        public bool UseSimpleAntiAliasing { get; set; } = false;
        public bool UsePathTracing { get; set; } = false;
        public int PathTracingSamples { get; set; } = 5;

    public RayTracing(Scene scene, int imgWidth, int imgHeight)
        {
            _scene = scene;
            _imgWidth = imgWidth;
            _imgHeight = imgHeight;
        }

        public Color CalculateColor(Vector2 v)
        {
            if (UseSimpleAntiAliasing)
            {
                var a = 5;
                int a2 = a * a;
                var step = 1 / (a - 1);

                (var sumR, var sumG, var sumB) = (0f, 0f, 0f);

                int i = 0;
                for (int x = 0; x < a; x++)
                {
                    for (int y = 0; y < a; y++, i++)
                    {
                        var ray = CreateEyeRay(new Vector2(v.X + step + x - a / 2, v.Y + step + y - a / 2));
                        var c = CalcColor(ray);

                        sumR += c.X;
                        sumG += c.Y;
                        sumB += c.Z;
                    }
                }

                return new Vector3(sumR / a2, sumG / a2, sumB / a2).ToColor();
            }
            else if (UseGaussAntiAliasing)
            {
                var omega = 0.5f;
                var mu = 1f;
                var a = 20;

                (var sumR, var sumG, var sumB) = (0f, 0f, 0f);

                for (int k = 0; k < a; k++)
                {
                    var x = Mathf.GaussDistribution(omega, mu);
                    var y = Mathf.GaussDistribution(omega, mu);

                    var ray = CreateEyeRay(new Vector2(v.X + x, v.Y + y));
                    var c = CalcColor(ray);

                    sumR += c.X;
                    sumG += c.Y;
                    sumB += c.Z;
                }

                return new Vector3(sumR / a, sumG / a, sumB / a).ToColor();
            }
            else if (Aperture > 0)
            {
                var a = 1000;

                (var sumR, var sumG, var sumB) = (0f, 0f, 0f);

                for (int i = 0; i < a; i++)
                {
                    var ray = CreateEyeRay(new Vector2(v.X, v.Y));
                    var c = CalcColor(ray);

                    sumR += c.X;
                    sumG += c.Y;
                    sumB += c.Z;
                }

                return new Vector3(sumR / a, sumG / a, sumB / a).ToColor();
            }
            else
            {
                var ray = CreateEyeRay(new Vector2(v.X, v.Y));
                return CalcColor(ray).ToColor();
            }
        }

        public Ray CreateEyeRay(Vector2 pixel)
        {
            var up = Vector3.UnitY;
            var f = _scene.LookAt - _scene.Eye;
            var fLeng = f.Length();

            var fNorm = Vector3.Normalize(f);
            var r = Vector3.Cross(up, fNorm);
            var u = -Vector3.Cross(r, fNorm);

            var h = 2f * (float)Math.Tan(_scene.Alpha / 2) * fLeng;

            var x = pixel.X * h / _imgWidth - h/2;
            var y = pixel.Y * h / _imgHeight - h/2;

            var d = f + (r * x) + (y * u);

            return Aperture > 0 
                ? DepthOfField(up, fLeng, d)
                : new Ray(_scene.Eye, d);
        }

        public Ray DepthOfField(Vector3 up, float fLeng, Vector3 d)
        {
            var r = Mathf.GetRandomRange(0, 1);
            var rSqrt = (float)Math.Sqrt(r);
            var omega = Mathf.GetRandomRange(0, 2 * Mathf.PI);

            (var x, var y) = (rSqrt * (float)Math.Sin(omega) * Aperture, rSqrt * (float)Math.Cos(omega) * Aperture);

            var fNorm = Vector3.Normalize(_scene.LookAt - _scene.Eye);
            var nx = Vector3.Normalize(Vector3.Cross(up, fNorm));
            var ny = Vector3.Normalize(Vector3.Cross(nx, fNorm));

            var a = nx * x + ny * y;

            var newEye = _scene.Eye + a;

            d = (_scene.Eye + d) - newEye;

            return new Ray(newEye, d);
        }

        public (Vector3? hit, IObject obj, float lambda) FindClosestHitPoint(Ray r)
        {
            var min = float.MaxValue;
            IObject minObj = null;
            foreach (var o in _scene.Objects)
            {
                var (l, obj) = o.ClosestIntersectionLambda(r);
                if (l < min)
                {
                    min = l;
                    minObj = obj;
                }
            }

            foreach (var plane in _scene.Planes)
            {
                var (l, obj) = plane.ClosestIntersectionLambda(r);
                if (l < min)
                {
                    min = l;
                    minObj = obj;
                }
            }

            if (minObj == null)
            {
                return (null, null, float.MaxValue);
            }

            return (r.StartPos + r.Direction * min, minObj, min);
        }
        
        public Vector3 CalcColor(Ray ray)
        {
            return UsePathTracing ? CalcColorPathTracing(ray, PathTracingSamples) : CalcColorRayTracing(ray);
        }
        

        public Vector3 CalcColorRayTracing(Ray ray, int recursionCount = 0)
        {
            (var hit, var obj, _) = FindClosestHitPoint(ray);
            if (!hit.HasValue) return Colors.Black.ToVector3();

            var Id = Vector3.Zero;
            var Is = Vector3.Zero;
            var Ir = Vector3.Zero;

            var phongShading = new PhongShading(obj, hit.Value, ray, this);

            foreach (var light in _scene.Lights)
            {
                var pLight = phongShading.LightShading(light);

                if (UseDiffues) Id += pLight.Diffuse();
                if (UseSpecular) Is += pLight.Specular();
            }

            if (UseReflection)
            {
                if (recursionCount < 3 && obj.Material.Kreflection > 0)
                {
                    (var kr, var reflectRay) = phongShading.KrAndReflectRay();

                    Ir = CalcColorRayTracing(reflectRay, recursionCount + 1) * kr;
                }
            }

            return _scene.EnviromentLight + Ir + Is + Id;
        }

        private readonly float pdf = 1f / (2f * Mathf.PI);

        public Vector3 CalcColorPathTracing(Ray ray, int samples, int recursionCount = 0)
        {
            (var hit, var obj, _) = FindClosestHitPoint(ray);
            if (!hit.HasValue) return Colors.Black.ToVector3();

            var color = obj.Material.GetColor(obj.ToLocalNormalized(hit.Value));

            var emission = obj.Material.Emission * color;

            if (emission.LengthSquared() > 0f) return emission * color;

            var n = obj.GetNorm(hit.Value);

            var sum = Vector3.Zero;

            if (recursionCount < 3)
            {
                for (int i = 0; i < samples; i++)
                {
                    var vec = Mathf.UniformRandom.NextVector(n);
                    var lightRay = new Ray(hit.Value + (vec * 0.0001f), vec);

                    sum += ((CalcColorPathTracing(lightRay, 4, recursionCount + 1) * Vector3.Dot(vec, n)) * color) / pdf;
                }
            }

            return emission + sum / samples;
        }
    }
}
