using System;
using System.Numerics;
using CornellBox.Models.Objects;

namespace CornellBox.Models
{
    public class PhongShading
    {
        private readonly Vector3 _n;
        private readonly Material _mat;
        private readonly Vector3 _hit;
        private readonly Vector3 _eh;
        private readonly RayTracing _rayTracing;
        private readonly Vector3 _localCoordinates;
        private readonly bool _isLightProbe;

        public PhongShading(IObject obj, Vector3 hit, Ray ray, RayTracing rayTracing)
        {
            _hit = hit;
            _n = obj.GetNorm(hit);
            _mat = obj.Material;
            _eh = ray.Direction;
            _rayTracing = rayTracing;
            _localCoordinates = obj.ToLocalNormalized(hit);
            _isLightProbe = obj is LightProbe;
        }

        public LightPhong LightShading(Light light) => new LightPhong(this, light);

        public (float kr, Ray ray) KrAndReflectRay()
        {
            var reflectVec = Vector3.Normalize(Vector3.Reflect(_eh, _n));
            var hPoint = _hit + reflectVec * 0.001f;

            var ray = new Ray(hPoint, reflectVec);
            var kr = _mat.Kreflection + (1 - _mat.Kreflection) * (float) Math.Pow(1 - Vector3.Dot(_n, reflectVec), 5);
            return (kr, ray);
        }

        public class LightPhong
        {
            private readonly Vector3 _l;
            private readonly Vector3 _n;
            private readonly Light _light;
            private readonly Material _mat;
            private readonly Vector3 _eh;
            private readonly Vector3 _hit;
            private readonly float _shadow;
            private readonly Vector3 _localCoordiantes;
            private readonly bool _isLightProbe;
            private readonly float _dotNL;

            public LightPhong(PhongShading phongShading, Light light)
            {
                _l = Vector3.Normalize(light.Position - phongShading._hit);
                _n = phongShading._n;
                _light = light;
                _mat = phongShading._mat;
                _eh = phongShading._eh;
                _hit = phongShading._hit;
                _shadow = Shadow(phongShading._rayTracing);
                _localCoordiantes = phongShading._localCoordinates;
                _isLightProbe = phongShading._isLightProbe;
                _dotNL = Vector3.Dot(_l, _n);
            }

            public Vector3 Diffuse()
            {
                if(_isLightProbe) return _mat.GetColor(_localCoordiantes);
                var id = _light.Intensity
                        * _light.Color
                        * Math.Max(0f, _dotNL)
                        * _shadow
                        * _mat.GetColor(_localCoordiantes);

                return LightAttenuation(id);
            }

            public Vector3 Specular()
            {
                if (_isLightProbe) return Vector3.Zero;

                var r = 2 * Math.Max(0f, _dotNL) * _n - _l;
                r = Vector3.Normalize(r);
                var ispec = _light.Intensity 
                    * _light.Color 
                    * (float)Math.Pow(Math.Max(0, -Vector3.Dot(r, _eh)), _mat.SpecularK) 
                    * _shadow
                    * _mat.Kspecular;

                return LightAttenuation(ispec);
            }

            private float Shadow(RayTracing rayTracing)
            {
                if (_isLightProbe || !rayTracing.UseShadows) return 1f;

                var shadowMultiplier = 1f;

                var sPoint = _hit + Vector3.Normalize(_light.Position - _hit) * 0.0001f + _n * 0.0001f; 
                (var lHit, _, var lambda) = rayTracing.FindClosestHitPoint(new Ray(sPoint, _l));

                if (lHit.HasValue && lambda * lambda < (_light.Position - _hit).LengthSquared())
                {
                    shadowMultiplier = 0.2f;
                }

                return shadowMultiplier;
            }

            private Vector3 LightAttenuation(Vector3 i)
            {
                var d = _l.LengthSquared();
                return i / (1 + _light.LightAttenuationC * d);
            }
        }
    }
}
