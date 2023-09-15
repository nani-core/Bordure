using UnityEngine;

namespace NaniCore.UnityPlayground {
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
	}
}