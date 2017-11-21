using System.Numerics;

namespace Triangles.Helpers
{
	public static partial class VectorHelper
	{
		public static Vector2 Lerp(Vector2 a, Vector2 b, Vector2 c, float u, float v)
		{
			return a + u * (b - a) + v * (c - a);
		}

		public static Vector3 Lerp(Vector3 a, Vector3 b, Vector3 c, float u, float v)
		{
			return a + u * (b - a) + v * (c - a);
		}

		public static Vector4 Lerp(Vector4 a, Vector4 b, Vector4 c, float u, float v)
		{
			return a + u * (b - a) + v * (c - a);
		}


		public static Vector3 LerpHomogeneous(Vector2 a, Vector2 b, Vector2 c, float wa, float wb, float wc, float u, float v)
		{
			var aa = new Vector3 (a / wa, 1 / wa);
			var bb = new Vector3 (b / wb, 1 / wb);
			var cc = new Vector3 (c / wc, 1 / wc);

			var vec = Lerp(aa, bb, cc, u, v);
			var vecZ = vec.Z;
			if(vecZ != 0) vec /= vecZ;
			vec.Z = vecZ;

			return vec;
		}

		public static Vector3 LerpHomogeneous(Vector2 a, Vector2 b, Vector2 c, (float a, float b, float c, float u, float v) wAndUv)
		{
			return LerpHomogeneous(a, b, c, wAndUv.a, wAndUv.b, wAndUv.c, wAndUv.u, wAndUv.v);
		}

		public static Vector4 LerpHomogeneous(Vector3 a, Vector3 b, Vector3 c, float wa, float wb, float wc, float u, float v)
		{
			var aa = new Vector4 (a / wa, 1 / wa);
			var bb = new Vector4 (b / wb, 1 / wb);
			var cc = new Vector4 (c / wc, 1 / wc);

			var vec = Lerp(aa, bb, cc, u, v);
			var vecW = vec.W;
			if(vecW != 0) vec /= vecW;
			vec.W = vecW;

			return vec;
		}

		public static Vector4 LerpHomogeneous(Vector3 a, Vector3 b, Vector3 c, (float a, float b, float c, float u, float v) wAndUv)
		{
			return LerpHomogeneous(a, b, c, wAndUv.a, wAndUv.b, wAndUv.c, wAndUv.u, wAndUv.v);
		}

	}
}
