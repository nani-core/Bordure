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
			if(UsesProtagonist) {
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

		public void AttachCameraTo(Transform transform, bool resetLocalTransforms = false) {
			mainCamera.transform.SetParent(transform, true);
			if(resetLocalTransforms) {
				mainCamera.transform.SetLocalPositionAndRotation(
					Vector3.zero,
					Quaternion.identity
				);
			}
		}

		public void RetrieveCameraHierarchy() {
			mainCamera.transform.SetParent(transform, true);
		}
		#endregion

		#region Functions
		private System.Collections.IEnumerator TransitCameraToCoroutine(
			Transform target,
			float duration,
			float easingFactor = 0.0f
		) {
			// Record protagonist control state before moving.
			bool usesMovement = false, usesOrientation = false;
			if(UsesProtagonist) {
				usesMovement = Protagonist.UsesMovement;
				usesOrientation = Protagonist.UsesOrientation;
				Protagonist.UsesMovement = false;
				Protagonist.UsesOrientation = false;
				Protagonist.IsKinematic = true;
			}

			// The moving process.
			{
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

			// Restore protagonist control state after moving.
			if(UsesProtagonist) {
				Protagonist.UsesMovement = usesMovement;
				Protagonist.UsesOrientation = usesOrientation;
			}
		}
		#endregion
	}
}