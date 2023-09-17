using System;

namespace NaniCore {
	[Serializable]
	public class FloatRange : Range<float> {
		public float min, max;

		public FloatRange() { }
		public FloatRange(float min, float max) {
			this.min = min;
			this.max = max;
		}
		public FloatRange(FloatRange r) : this(r.min, r.max) { }

		public float Min => min;
		public float Max => max;

		public bool Contains(float value) {
			return Min <= value && Max >= value;
		}
	}

	[Serializable]
	public class FloatPivotRange : FloatRange, PivotRange<float> {
		public float pivot;

		public FloatPivotRange() { }
		public FloatPivotRange(float min, float max, float pivot): base(min, max) {
			this.pivot = pivot;
		}
		public FloatPivotRange(FloatPivotRange r) : this(r.min, r.max, r.pivot) { }
		public FloatPivotRange(FloatRange r, float pivot) : this(r.min, r.max, pivot) { }

		public float Pivot => pivot;
	}
}