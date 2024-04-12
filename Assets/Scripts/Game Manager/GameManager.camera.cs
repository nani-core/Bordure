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
		public void AlignCameraTo(Transform target) {
			if(IsUsingProtagonist) {
				Protagonist.transform.position = target.position - (Protagonist.Eye.position - Protagonist.transform.position);
				Vector3 euler = target.rotation.eulerAngles * Mathf.Deg2Rad;
				Protagonist.Azimuth = euler.y;
				Protagonist.Zenith = euler.x;
			}
			else {
				AttachCameraTo(target, true);
				RetrieveCameraHierarchy();
			}
		}

		public void LerpCameraTo(Transform target) {
			StartCoroutine(LerpCameraToCoroutine(target, 1f));
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

		private System.Collections.IEnumerator LerpCameraToCoroutine(
			Transform target,
			float duration,
			float easingFactor = 0.0f
		) {
			Transform
				start = new GameObject("Lerp Start").transform,
				end = new GameObject("Lerp End").transform,
				anchor = new GameObject("Lerp Anchor").transform;
			start.AlignWith(MainCamera.transform);
			end.AlignWith(target);

			yield return MathUtility.ProgressCoroutine(
				duration,
				progress => {
					anchor.Lerp(start, end, progress);
					AlignCameraTo(anchor);
				},
				easingFactor
			);

			Destroy(start.gameObject);
			Destroy(end.gameObject);
			Destroy(anchor.gameObject);
		}
		#endregion
	}
}