using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Schema;

namespace Triangles.Helpers
{
    public static class ArrayMath
    {
        public static void Mul(float[] a, float[] b, float[] result)
        {
            Contract.Assert(a.Length != b.Length, "a.Length != b.Length");
            Contract.Assert(a.Length != result.Length, "a.Length != result.Length");

            for (int i = 0; i < a.Length; i++)
            {
                result[i] = a[i] * b[i];
            }
        }

        public static float[] Mul(float[] a, float[] b)
        {
            var r = new float[a.Length];
            Mul(a, b, r);
            return r;
        }
 

        public static float Dot(float[] a, float[] b)
        {
            Contract.Assert(a.Length != b.Length, "a.Length != b.Length");

            var r = 0f;
            for (int i = 0; i < a.Length; i++)
            {
                r += a[i] * b[i];
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

            float lenght = CMathLib.CMathF.SqrtF(ls);

            for (int i = 0; i < a.Length; i++)
            {
                a[i] /= lenght;
            }
        }

        public static void Add(float[] a, float b, float[] r)
        {
            for (int i = 0; i < a.Length; i++)
            {
                r[i] += b;
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

        public static void Sub(float[] a, float[] b, float[] r)
        {
            for (int i = 0; i < a.Length; i++)
            {
                r[i] = a[i] - b[i];
            }
        }

        public static float[] Sub(float[] a, float[] b)
        {
            var r = new float[a.Length];
            Sub(a, b, r);
            return r;
        }

        public static void Mul(float[] a, float b, float[] r)
        {
            for (int i = 0; i < a.Length; i++)
            {
                r[i] = a[i] * b;
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
    }
}
