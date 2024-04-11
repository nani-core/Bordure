using UnityEngine;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Fields
		[SerializeField] private Camera mainCamera;
		#endregion

		#region Interfaces
		public Camera MainCamera => mainCamera;

		/// <summary>
		/// Align the camera, along with the protagonist if it exists, to the
		/// target transform.
		/// </summary>
		public void AlignCameraTo(Transform transform) {
			if(Protagonist == null) {
				AttachCameraTo(transform, true);
				RetrieveCameraHierarchy();
			}
			else {
				Protagonist.transform.position = transform.position - (Protagonist.Eye.position - Protagonist.transform.position);
				Vector3 euler = transform.rotation.eulerAngles * Mathf.Deg2Rad;
				Protagonist.Azimuth = euler.y;
				Protagonist.Zenith = euler.x;
			}
		}
		#endregion

		#region Functions
		private void AttachCameraTo(Transform transform, bool resetLocalTransforms = false) {
			mainCamera.transform.SetParent(transform, true);
			if(resetLocalTransforms) {
				mainCamera.transform.SetLocalPositionAndRotation(
					Vector3.zero,
					Quaternion.identity
				);
			}
		}

		private void RetrieveCameraHierarchy() {
			mainCamera.transform.SetParent(transform, true);
		}
		#endregion
	}
}