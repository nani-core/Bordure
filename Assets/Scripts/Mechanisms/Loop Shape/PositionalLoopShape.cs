using UnityEngine;
using NaughtyAttributes;

namespace NaniCore.Loopool {
	public partial class PositionalLoopShape : LoopShape {
		#region Serialized fields
		/// »·
		[SerializeField][Tooltip("»·")] private GameObject blasto;
		/// µº
		[SerializeField][Tooltip("µº")] private GameObject gastro;

		[Header("Positioning")]
		[SerializeField][MinMaxSlider(-90, 90)] private Vector2 allowedPositioningAzimuth;
		[SerializeField][MinMaxSlider(0, 1)] private Vector2 allowedPositioningDistanceRatio;

		[Header("Placement")]
		[SerializeField][Range(0, 1)] private float idealPlacementDistanceRatio = .5f;
		[SerializeField][MinMaxSlider(0, 1)] private Vector2 allowedPlacementDistanceRatio;
		[SerializeField][Range(-180, 180)] private float idealPlacementAzimuth = 0f;
		[SerializeField][MinMaxSlider(-90, 90)] private Vector2 allowedPlacementAzimuth;
		#endregion

		#region Properties
		private Vector3 BlastoPosition => blasto.transform.position;
		private Vector3 InitialViewDirection => BlastoPosition - InitialViewPosition;
		private Vector3 InitialViewPosition => transform.position;
		private Quaternion IdealPlacementRotation => Quaternion.Euler(0, idealPlacementAzimuth, 0) * Quaternion.LookRotation(InitialViewDirection, -Physics.gravity);
		private Matrix4x4 BlastoToWorldMatrix => Matrix4x4.TRS(BlastoPosition, Quaternion.LookRotation(-InitialViewDirection, -Physics.gravity), Vector3.one);
		#endregion

		#region Functions
		private Vector3 GetPositionAlongViewingLine(float ratio) {
			if(blasto == null)
				return transform.position;
			return Vector3.Lerp(InitialViewPosition, BlastoPosition, ratio);
		}

		public override bool Satisfied(Transform eye) {
			if(blasto == null || gastro == null)
				return false;

			// Validate positioning
			var positioningCc = (MathUtility.CylindricalCoordinate)BlastoToWorldMatrix.inverse.MultiplyPoint(eye.position);
			positioningCc.radius /= InitialViewDirection.magnitude;
			bool positioningRadiusInRange = positioningCc.radius.InRange(allowedPositioningDistanceRatio);
			bool positioningAzimuthInRange = (positioningCc.azimuth * 180 / Mathf.PI).InRange(allowedPositioningAzimuth);	// Should be fixed for radians.
			if(!(positioningRadiusInRange && positioningAzimuthInRange))
				return false;

			// Validate placement
			var gt = gastro.transform;
			var placementCc = (MathUtility.CylindricalCoordinate)eye.worldToLocalMatrix.MultiplyPoint(gt.position);
			placementCc.radius /= Vector3.Distance(eye.position, BlastoPosition);
			bool placementRadiusInRange = placementCc.radius.InRange(allowedPlacementDistanceRatio);
			bool placementAzimuthInRange = placementCc.azimuth.InRange(allowedPlacementAzimuth);
			if(!(placementRadiusInRange && placementAzimuthInRange))
				return false;

			return true;
		}
		#endregion
	}
}