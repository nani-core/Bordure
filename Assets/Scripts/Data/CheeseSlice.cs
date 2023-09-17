using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NaniCore.Loopool {
	using CC = MathUtility.CylindricalCoordinate;

	public abstract class CheeseSlice {
		public abstract FloatRange Azimuth { get; }
		public abstract FloatRange DistanceRatio { get; }

		public bool Check(Vector3 from, Quaternion rotation, Vector3 to, Vector3 targetPosition, Quaternion targetRotation) {
			float distance = Vector3.Distance(from, targetPosition) / Vector3.Distance(from, to);
			if(!DistanceRatio.Contains(distance))
				return false;
			float azimuth = (targetRotation * Quaternion.Inverse(rotation)).eulerAngles.y;
			if(!MathUtility.DegreeInRange(azimuth, Azimuth.min, Azimuth.max))
				return false;
			return true;
		}

#if UNITY_EDITOR
		public virtual void DrawGizmos(Vector3 from, Quaternion rotation, Vector3 to) {
			var azimuthRadian = new FloatRange(Azimuth);
			azimuthRadian.min *= Mathf.PI / 180;
			azimuthRadian.max *= Mathf.PI / 180;
			var distanceRatio = DistanceRatio;
			var rawVertices = new List<CC> {
						new CC(distanceRatio.min, azimuthRadian.min, 0),
						new CC(distanceRatio.min, 0, 0),
						new CC(distanceRatio.min, azimuthRadian.max, 0),
						new CC(distanceRatio.max, azimuthRadian.max, 0),
						new CC(distanceRatio.max, 0, 0),
						new CC(distanceRatio.max, azimuthRadian.min, 0),
					};
			float distance = Vector3.Distance(from, to);
			GizmosUtility.DrawPolygon(rawVertices.Select(cc => {
				cc.radius *= distance;
				return from + rotation * (Vector3)cc;
			}));
		}
#endif
	}

	[Serializable]
	public class NoPivotCheeseSlice : CheeseSlice {
		[SerializeReference][Range(-90, 90)] public FloatRange azimuth = new FloatRange(-5f, 5f);
		[SerializeReference][Range(0, 1)] public FloatRange distanceRatio = new FloatRange(.25f, 1f);

		public override FloatRange Azimuth => azimuth;
		public override FloatRange DistanceRatio => distanceRatio;
	}

	[Serializable]
	public class PivotCheeseSlice : CheeseSlice {
		[SerializeReference][Range(-90, 90)] public FloatPivotRange azimuth = new FloatPivotRange(-5f, 5f, 0f);
		[SerializeReference][Range(0, 1)] public FloatPivotRange distanceRatio = new FloatPivotRange(.25f, 1f, .5f);

		public override FloatRange Azimuth => azimuth;
		public override FloatRange DistanceRatio => distanceRatio;
	}
}