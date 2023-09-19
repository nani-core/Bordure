using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NaniCore.Loopool {
	using SC = MathUtility.SphericalCoordinate;

	public abstract class CheeseSlice {
		public abstract FloatRange Azimuth { get; }
		public abstract FloatRange Zenith { get; }
		public abstract FloatRange DistanceRatio { get; }

		public bool Check(Vector3 from, Quaternion rotation, Vector3 to, Vector3 targetPosition, Quaternion targetRotation, bool validateRelativePlacement = false) {
			// Validate distance range
			float distance = Vector3.Distance(from, targetPosition) / Vector3.Distance(from, to);
			if(!DistanceRatio.Contains(distance))
				return false;
			// Validate target orientation
			float azimuth = (targetRotation * Quaternion.Inverse(rotation)).eulerAngles.y;
			if(!MathUtility.DegreeInRange(azimuth, Azimuth.min, Azimuth.max))
				return false;
			float zenith = (targetRotation * Quaternion.Inverse(rotation)).eulerAngles.x;
			if(!MathUtility.DegreeInRange(zenith, Zenith.min, Zenith.max))
				return false;
			// Validate target relative placement
			if(validateRelativePlacement) {
				Vector3 relativePos = Quaternion.Inverse(rotation) * (targetPosition - from);
				float relativeAzimuth = Mathf.Atan2(relativePos.x, relativePos.z) * 180 / Mathf.PI;
				if(!MathUtility.DegreeInRange(relativeAzimuth, Azimuth.min, Azimuth.max))
					return false;
			}
			return true;
		}

#if UNITY_EDITOR
		private void DrawFan(Vector3 from, Quaternion rotation, Vector3 to, float zenith) {
			var azimuthRadian = new FloatRange(Azimuth);
			azimuthRadian.min *= Mathf.PI / 180;
			azimuthRadian.max *= Mathf.PI / 180;
			var distanceRatio = DistanceRatio;
			var rawVertices = new List<SC> {
						new SC(distanceRatio.min, azimuthRadian.min, zenith),
						new SC(distanceRatio.min, 0, zenith),
						new SC(distanceRatio.min, azimuthRadian.max, zenith),
						new SC(distanceRatio.max, azimuthRadian.max, zenith),
						new SC(distanceRatio.max, 0, zenith),
						new SC(distanceRatio.max, azimuthRadian.min, zenith),
					};
			float distance = Vector3.Distance(from, to);
			GizmosUtility.DrawPolygon(rawVertices.Select(cc => {
				cc.radius *= distance;
				return from + rotation * (Vector3)cc;
			}));
		}
			
		public virtual void DrawGizmos(Vector3 from, Quaternion rotation, Vector3 to) {
			var zenithRadian = new FloatRange(Zenith);
			zenithRadian.min *= Mathf.PI / 180;
			zenithRadian.max *= Mathf.PI / 180;
			DrawFan(from, rotation, to, zenithRadian.min);
			DrawFan(from, rotation, to, 0f);
			DrawFan(from, rotation, to, zenithRadian.max);
		}
#endif
	}

	[Serializable]
	public class NoPivotCheeseSlice : CheeseSlice {
		[SerializeReference][Range(-90, 90)] public FloatRange azimuth = new FloatRange(-5f, 5f);
		[SerializeReference][Range(-90, 90)] public FloatRange zenith = new FloatRange(-5f, 5f);
		[SerializeReference][Range(0, 1)] public FloatRange distanceRatio = new FloatRange(.25f, 1f);

		public override FloatRange Azimuth => azimuth;
		public override FloatRange Zenith => zenith;
		public override FloatRange DistanceRatio => distanceRatio;
	}

	[Serializable]
	public class PivotCheeseSlice : CheeseSlice {
		[SerializeReference][Range(-90, 90)] public FloatPivotRange azimuth = new FloatPivotRange(-5f, 5f, 0f);
		[SerializeReference][Range(-90, 90)] public FloatPivotRange zenith = new FloatPivotRange(-5f, 5f, 0f);
		[SerializeReference][Range(0, 1)] public FloatPivotRange distanceRatio = new FloatPivotRange(.25f, 1f, .5f);

		public override FloatRange Azimuth => azimuth;
		public override FloatRange Zenith => zenith;
		public override FloatRange DistanceRatio => distanceRatio;
	}
}