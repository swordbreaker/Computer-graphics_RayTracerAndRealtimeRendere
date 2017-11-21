using System;
using System.Numerics;
using CMathLib;

namespace CornellBox.Helpers
{
    public static class Mathf
    {
        public const float Epsilon = 0.0000001f;
        public static readonly float PI = 3.1415926f;

        private static readonly Random _random = new Random();

        public static float ToRadian(float v) => v / 180f * Mathf.PI;
        public static float ToDeg(float v) => v / Mathf.PI * 180f;

        public static readonly UniformSamples UniformRandom = new UniformSamples(1000);

        public static float Sqrt(float x) => CMathF.SqrtF(x);
        public static float Round(float x) => CMathF.Round(x);
        public static float Atan2(float x, float y) => CMathF.Atan2(x, y);
        public static float Acos(float x) => CMathF.Acos(x);

        public static float GaussDistribution(float omega, float mu)
        {
            var x = UniformRandom.Next();

            var a = Math.Pow((x - mu) / omega, 2);
            var b = Math.Pow(Math.E, -0.5f * a);
            var c = 1 / (omega * Math.Sqrt(2f * PI));

            return (float)(c * b);
        }

        public static float GetRandomRange(float min, float max)
        {
            lock (_random)
            {
                return (float)(min + _random.NextDouble() * (max+1));
            }
        }

        private static float GetRandomRangeNotThreadSafe(float min, float max) => (float)(min + _random.NextDouble() * (max + 1));

        public class UniformSamples
        {
            private readonly float[] _samples;
            private int _i = 0;

            public UniformSamples(int count)
            {
                _samples = new float[count];
                for (int i = 0; i < count; i++)
                {
                    _samples[i] = GetRandomRangeNotThreadSafe(-1, 1);
                }
            }

            public float Next()
            {
                _i = (_i + 1) % _samples.Length;
                return _samples[_i];
            }

            public Vector3 NextVector(Vector3 n)
            {
                Vector3 vec;
                do
                {
                    var x = Next();
                    var y = Next();
                    var z = Next();

                    vec = new Vector3(x, y, z);
                    if (Vector3.Dot(n, vec) <= 0) vec = -vec;
                } while (vec.LengthSquared() > 1);

                return Vector3.Normalize(vec);
            }
        }
    }


}
