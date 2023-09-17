using UnityEngine;

namespace NaniCore.Loopool {
	public partial class PositionalLoopShape : LoopShape {
		#region Serialized fields
		/// »·
		[SerializeField][Tooltip("»·")] private GameObject blasto;
		/// µº
		[SerializeField][Tooltip("µº")] private GameObject gastro;

		[SerializeField] private NoPivotCheeseSlice positioning;
		[SerializeField] private PivotCheeseSlice placement;
		#endregion

		#region Properties
		private Vector3 BlastoPosition => blasto.transform.position;
		private Vector3 InitialViewDirection => BlastoPosition - InitialViewPosition;
		private Vector3 InitialViewPosition => transform.position;
		private Quaternion IdealPlacementRotation => Quaternion.Euler(0, placement.azimuth.pivot, 0) * Quaternion.LookRotation(InitialViewDirection, -Physics.gravity);
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
			bool positioningRadiusInRange = positioning.distanceRatio.Contains(positioningCc.radius);
			bool positioningAzimuthInRange = positioning.azimuth.Contains(positioningCc.azimuth * 180 / Mathf.PI);   // Should be fixed for radians.
			if(!(positioningRadiusInRange && positioningAzimuthInRange))
				return false;

			// Validate placement
			var gt = gastro.transform;
			var placementCc = (MathUtility.CylindricalCoordinate)eye.worldToLocalMatrix.MultiplyPoint(gt.position);
			placementCc.radius /= Vector3.Distance(eye.position, BlastoPosition);
			bool placementRadiusInRange = placement.distanceRatio.Contains(placementCc.radius);
			bool placementAzimuthInRange = placement.azimuth.Contains(placementCc.azimuth * 180 / Mathf.PI);
			if(!(placementRadiusInRange && placementAzimuthInRange))
				return false;

			return true;
		}

		public void DestroyGastro() {
			Destroy(gastro.gameObject);
		}
		#endregion
	}
}