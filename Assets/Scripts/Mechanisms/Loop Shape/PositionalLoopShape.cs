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
		private Vector3 ViewDirection => BlastoPosition - transform.position;
		private Quaternion IdealPlacementRotation => Quaternion.Euler(0, placement.azimuth.pivot, 0) * Quaternion.LookRotation(ViewDirection, -Physics.gravity);
		#endregion

		#region Functions
		private Vector3 GetPositionAlongViewingLine(float ratio) {
			if(blasto == null)
				return transform.position;
			return Vector3.Lerp(transform.position, BlastoPosition, ratio);
		}

		public override bool Satisfied(Transform eye) {
			if(blasto == null || gastro == null)
				return false;

			// Validate positioning
			Vector3 inversedEyeRotation = eye.rotation.eulerAngles;
			inversedEyeRotation.y += 180;
			if(!positioning.Check(blasto.transform.position, Quaternion.LookRotation(-ViewDirection), transform.position, eye.position, Quaternion.Euler(inversedEyeRotation)))
				return false;

			// Validate placement
			if(!placement.Check(eye.position, Quaternion.LookRotation(ViewDirection), blasto.transform.position, gastro.transform.position, gastro.transform.rotation))
				return false;

			return true;
		}

		public void DestroyGastro() {
			if(gastro == null)
				return;
			Destroy(gastro.gameObject);
			gastro = null;
		}
		#endregion
	}
}