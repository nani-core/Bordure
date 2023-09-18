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

		[Serializable]
		public struct SphericalCoordinate {
			public float radius;
			public float azimuth;
			public float zenith;

			public SphericalCoordinate(float radius, float azimuth, float zenith) {
				this.radius = radius;
				this.azimuth = azimuth;
				this.zenith = zenith;
			}

			public static SphericalCoordinate FromCartesian(Vector3 cartesian) {
				float azimuth = Mathf.Atan2(cartesian.x, cartesian.z);
				float radius = cartesian.magnitude;
				float y = cartesian.y;
				cartesian.y = 0;
				float zenith = Mathf.Atan2(y, cartesian.magnitude);
				return new SphericalCoordinate(radius, azimuth, zenith);
			}

			public static Vector3 ToCartesian(SphericalCoordinate cylindrical) {
				Vector3 result = Vector3.zero;
				result.z = Mathf.Cos(cylindrical.azimuth);
				result.x = Mathf.Sin(cylindrical.azimuth);
				result *= Mathf.Cos(cylindrical.zenith);
				result.y = Mathf.Sin(cylindrical.zenith);
				result *= cylindrical.radius;
				return result;
			}

			public static explicit operator SphericalCoordinate(Vector3 cartesian)
				=> FromCartesian(cartesian);

			public static explicit operator Vector3(SphericalCoordinate spherical)
				=> ToCartesian(spherical);
		}
	}
}