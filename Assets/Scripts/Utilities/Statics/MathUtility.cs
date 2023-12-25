using UnityEngine;
using System;

namespace NaniCore {
	public static class MathUtility {
		public static float Ease(float t, float alpha) {
			float power = (1 - alpha) / (1 + alpha);
			t = t * 2 - 1;
			float sgn = Mathf.Sign(t);
			t *= sgn;
			t = Mathf.Pow(t, power);
			t *= sgn;
			t = (t + 1) * .5f;
			return t;
		}

		public static float InvertColorChannel(float f) {
			int i = Mathf.FloorToInt(f * 256);
			i ^= 0xff;
			return (float)i / 256;
		}

		public static Color Invert(this Color c) {
			c.r = InvertColorChannel(c.r);
			c.g = InvertColorChannel(c.g);
			c.b = InvertColorChannel(c.b);
			return c;
		}

		public static bool IsSameHex(Color a, Color b) {
			return Vector4.Distance(a, b) < 1f / 256;
		}

		public static Matrix4x4 RelativeTransform(Matrix4x4 from, Matrix4x4 to)
			=> to.inverse * from;

		/// <summary>
		/// Given two transforms, returns a matrix that maps the local space of
		/// the self transform to the local space of the target transform.
		/// </summary>
		public static Matrix4x4 RelativeTransform(this Transform self, Transform target)
			=> RelativeTransform(self.localToWorldMatrix, target.localToWorldMatrix);

		public static bool InRange(this float x, float min, float max)
			=> x >= min && x <= max;

		public static bool InRange(this float x, Vector2 range)
			=> x.InRange(range.x, range.y);

		public static float Residue(this float x, float divisor)
			=> Mathf.Floor(x / divisor);

		public static float Mod(this float x, float divisor)
			=> x - divisor * x.Residue(divisor);

		public static bool DegreeInRange(float x, float min, float max) {
			if(max < min) {
				float t = max;
				max = min;
				min = t;
			}
			float minRes = min.Residue(360);
			min += minRes;
			max += minRes;
			x += x.Residue(360);
			return x.InRange(min, max) || (x + 360).InRange(min, max) || (x - 360).InRange(min, max);
		}

		public static Vector2Int Floor(this Vector2 v) {
			return new Vector2Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y));
		}

		public static Vector2Int Ceil(this Vector2 v) {
			return new Vector2Int(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y));
		}

		public static Vector3 ProjectOntoAxis(this Vector3 from, Vector3 to) {
			var norm = to.normalized;
			return Vector3.Dot(from, norm) * norm;
		}

		public static Vector3 ProjectOntoPlane(this Vector3 from, Vector3 to) {
			return from - from.ProjectOntoAxis(to);
		}

		public static float SnapToZero(this float value, float tolerance = .01f) {
			if(Mathf.Abs(value) <= Mathf.Abs(tolerance))
				return 0f;
			return value;
		}
	}
}