using UnityEngine;

namespace NaniCore.Loopool {
	public partial class PositionalLoopShape : LoopShape {
		#region Serialized fields
		[Header("Geometry")]
		[SerializeField] public Transform origin;
		[SerializeField] public NoPivotCheeseSlice positioning;
		[SerializeField] public PivotCheeseSlice placement;
		#endregion

		#region Properties
		public Vector3 OriginPos => origin.position;
		public Vector3 BlastoPos => blasto.transform.position;
		public Vector3 GastroPos => gastro.transform.position;
		#endregion

		#region Functions
		private Vector3 GetPositionAlongViewingLine(float ratio) {
			if(blasto == null)
				return OriginPos;
			return Vector3.Lerp(OriginPos, BlastoPos, ratio);
		}

		public override bool Validate(Transform eye) {
			if(blasto == null || gastro == null)
				return false;

			// Validate positioning
			Vector3 inversedEyeRotation = eye.rotation.eulerAngles;
			inversedEyeRotation.y += 180;
			if(!positioning.Check(BlastoPos, Quaternion.LookRotation(OriginPos - BlastoPos), OriginPos, eye.position, Quaternion.Euler(inversedEyeRotation)))
				return false;

			// Validate placement
			if(!placement.Check(eye.position, Quaternion.LookRotation(BlastoPos - OriginPos), BlastoPos, GastroPos, gastro.transform.rotation, true))
				return false;

			return true;
		}
		#endregion
	}
}