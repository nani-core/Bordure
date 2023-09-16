using UnityEngine;

namespace NaniCore.Loopool {
	public partial class PositionalLoopShape : LoopShape {
		#region Serialized fields
		[SerializeField][Tooltip("»·")] private GameObject blasto;
		[SerializeField][Tooltip("µº")] private GameObject gastro;
		[SerializeField][Range(0, 1)] private float idealPlacementRatio = .5f;
		[SerializeField][Range(-180, 180)] private float idealPlacementAzimuth = 0f;
		#endregion

		#region Properties
		public Vector3 IdealPosition {
			get {
				if(blasto == null)
					return transform.position;
				return Vector3.Lerp(transform.position, blasto.transform.position, idealPlacementRatio);
			}
		}

		public Quaternion IdealRotation {
			get {
				return Quaternion.Euler(0, idealPlacementAzimuth, 0) * transform.rotation;
			}
		}
		#endregion
	}
}