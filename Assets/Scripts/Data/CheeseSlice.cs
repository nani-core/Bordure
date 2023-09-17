using System;
using UnityEngine;

namespace NaniCore.Loopool {
	public abstract class CheeseSlice {
		public abstract Range<float> Azimuth { get; }
		public abstract Range<float> DistanceRatio { get; }
	}

	[Serializable]
	public class NoPivotCheeseSlice : CheeseSlice {
		[SerializeReference][Range(-90, 90)] public FloatRange azimuth = new FloatRange(-5f, 5f);
		[SerializeReference][Range(0, 1)] public FloatRange distanceRatio = new FloatRange(.25f, 1f);

		public override Range<float> Azimuth => azimuth;
		public override Range<float> DistanceRatio => distanceRatio;
	}

	[Serializable]
	public class PivotCheeseSlice : CheeseSlice {
		[SerializeReference][Range(-90, 90)] public FloatPivotRange azimuth = new FloatPivotRange(-5f, 5f, 0f);
		[SerializeReference][Range(0, 1)] public FloatPivotRange distanceRatio = new FloatPivotRange(.25f, 1f, .5f);

		public override Range<float> Azimuth => azimuth;
		public override Range<float> DistanceRatio => distanceRatio;
	}
}