using System;

namespace Triangles.Helpers
{
    public static class ArrayMath
    {
        public static void Mul(float[] a, float[] b, float[] result, int aFrom, int bFrom, int rFrom, int lenght)
        {
            int ri = rFrom;
            int bi = bFrom;
            for (int ai = aFrom; ai < aFrom + lenght; ai++, bi++, ri++)
            {
                result[ri] = a[ai] * b[bi];
            }
        }

        public static void Mul(float[] a, float[] b, float[] result) => Mul(a, b, result, 0, 0, 0, a.Length);

        public static float[] Mul(float[] a, float[] b)
        {
            var r = new float[a.Length];
            Mul(a, b, r, 0, 0, 0, a.Length);
            return r;
        }
 

        public static float Dot(float[] a, float[] b)
        {
            var r = 0f;
            for (int i = 0; i < a.Length; i++)
            {
                r += a[i] * b[i];
            }
            return r;
        }

        public static float Dot(float[] a, float[] b, int aFrom, int bFrom, int lenght)
        {
            var r = 0f;
            var bi = bFrom;
            for (int ai = aFrom; ai < aFrom + lenght; ai++, bi++)
            {
                r += a[ai] * b[bi];
            }
            return r;
        }

        public static void Normalize(float[] a)
        {
            float ls = 0;
            for (int i = 0; i < a.Length; i++)
            {
                ls += a[i] * a[i];
            }

            float lenght = (float)Math.Sqrt(ls);

            for (int i = 0; i < a.Length; i++)
            {
                a[i] /= lenght;
            }
        }

        public static void Normalize(float[] a, int from, int lenght)
        {
            float ls = 0;
            for (int i = from; i < from + lenght; i++)
            {
                ls += a[i] * a[i];
            }

            float l = (float)Math.Sqrt(ls);

            for (int i = from; i < from + lenght; i++)
            {
                a[i] /= l;
            }
        }

        public static void Add(float[] a, float[] b, float[] r, int aFrom, int bFrom, int rFrom, int lenght)
        {
            var bi = bFrom;
            var ri = rFrom;

            for (int ai = aFrom; ai < aFrom + lenght; ai++, bi++, ri++)
            {
                r[ri] = a[ai] + b[bi];
            }
        }

        public static void Add(float[] a, float b, float[] r, int aFrom, int lenght)
        {
            for (int i = aFrom; i < aFrom + lenght; i++)
            {
                r[i] = a[i] + b;
            }
        }


        public static void Add(float[] a, float b, float[] r)
        {
            for (int i = 0; i < a.Length; i++)
            {
                r[i] = a[i] + b;
            }
        }

        public static float[] Add(float[] a, float b)
        {
            var r = new float[a.Length];
            Add(a, b, r);
            return r;
        }

        public static void Add(float[] a, float[] b, float[] r)
        {
            for (int i = 0; i < a.Length; i++)
            {
                r[i] = a[i] + b[i];
            }
        }

        public static float[] Add(float[] a, float[] b)
        {
            var r = new float[a.Length];
            Add(a, b, r);
            return r;
        }

        public static void Sub(float[] a, float[] b, float[] r, int aStart, int bStart, int rStart, int lenght)
        {
            int k = rStart;
            int bi = bStart;
            for (int ai = aStart; ai < aStart + lenght; ai++, bi++, k++)
            {
                r[k] = a[ai] - b[bi];
            }
        }

        public static void Sub(float[] a, float[] b, float[] r) => Sub(a, b, r, 0, 0, 0, a.Length);

        public static float[] Sub(float[] a, float[] b, int aStart, int bStart, int lenght)
        {
            var r = new float[a.Length];
            Sub(a, b, r, aStart, bStart, 0, lenght);
            return r;
        }

        public static float[] Sub(float[] a, float[] b) => Sub(a, b, 0, 0, a.Length);

        public static void Mul(float[] a, float b, float[] r)
        {
            for (int i = 0; i < a.Length; i++)
            {
                r[i] = a[i] * b;
            }
        }

        public static void Mul(float[] a, float b, float[] r, int aFrom, int rFrom, int length)
        {
            var ri = rFrom;
            for (int ai = aFrom; ai < aFrom + length; ai++, ri++)
            {
                r[ri] = a[ai] * b;
            }
        }

        public static float[] Mul(float[] a, float b)
        {
            var r = new float[a.Length];
            Mul(a, b, r);
            return r;
        }

        public static float[] Lerp(float[] a, float[] b, float[] c, float u, float v)
        {
            var q = Add(Mul(Sub(b, a), u), a);
            var w = Mul(Sub(c, a), v);
            return Add(q, w);
        }

        public static float[] NormalizeW(float[] a, float w)
        {
            var aa = new float[a.Length + 1];
            for (int i = 0; i < a.Length; i++)
            {
                aa[i] = a[i] / w;
            }
            aa[a.Length] = 1 / w;
            return aa;
        }

        public static float[] LerpHomogeneous(float[] a, float[] b, float[] c, float wa, float wb, float wc, float u, float v)
        {
            var aa = NormalizeW(a, wa);
            var bb = NormalizeW(b, wb);
            var cc = NormalizeW(c, wc);

            var vec = Lerp(aa, bb, cc, u, v);
            var w = vec[vec.Length - 1];
            if (w != 0) Mul(vec, 1 / w, vec);
            vec[vec.Length - 1] = w;

            return vec;
            //var aa = new Vector3(a / wa, 1 / wa);
            //var bb = new Vector3(b / wb, 1 / wb);
            //var cc = new Vector3(c / wc, 1 / wc);

            //var vec = Lerp(aa, bb, cc, u, v);
            //var vecZ = vec.Z;
            //if (vecZ != 0) vec /= vecZ;
            //vec.Z = vecZ;

            //return vec;
        }

        public static void Clamp(float[] a, float min, float max, int from, int lengt)
        {
            for (int i = from; i < from + lengt; i++)
            {
                if (a[i] < min) a[i] = min;
                if (a[i] > max) a[i] = max;
            }
        }
    }
}
