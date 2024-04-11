using UnityEngine;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Fields
		[SerializeField] private Camera mainCamera;
		#endregion

		#region Interfaces
		public Camera MainCamera => mainCamera;
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