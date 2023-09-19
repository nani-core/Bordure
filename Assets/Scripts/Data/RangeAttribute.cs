using UnityEngine;
using System;

namespace NaniCore {
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class RangeAttribute : PropertyAttribute {
		public float min, max;

		public RangeAttribute(float min, float max) {
			this.min = min;
			this.max = max;
		}
	}
}