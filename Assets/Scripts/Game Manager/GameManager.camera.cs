using UnityEngine;

namespace NaniCore.Bordure {
	public partial class GameManager {
		#region Fields
		[SerializeField] private MainCameraManager mainCameraManager;
		#endregion

		#region Interfaces
		public Camera MainCamera => mainCameraManager.Camera;

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
			mainCameraManager.transform.SetParent(transform, true);
			if(resetLocalTransforms) {
				mainCameraManager.transform.SetLocalPositionAndRotation(
					Vector3.zero,
					Quaternion.identity
				);
			}
		}

		public void RetrieveCameraHierarchy() {
			mainCameraManager.transform.SetParent(transform, true);
		}

		public Coroutine TransitCameraTo(Transform target, float duration, float easingFactor) {
			return StartCoroutine(TransitCameraToCoroutine(target, duration, easingFactor));
		}
		public Coroutine TransitCameraTo(Transform target) => TransitCameraTo(target, 1.0f, 0.0f);

		public Coroutine BlendToCamera(Camera target, float duration) {
			if(target == null) {
				Debug.LogWarning("Warning: The target blending camera is null.");
				return null;
			}
			return StartCoroutine(BlendToCameraCoroutine(target, duration));
		}
		public Coroutine BlendToCamera(Camera target) => BlendToCamera(target, 1.0f);
		public Coroutine BlendToCameraByName(string name) {
			Camera target = HierarchyUtility.FindObjectByName<Camera>(name, true);
			if(target == null) {
				Debug.LogWarning($"Warning: Cannot find the target blending camera (\"{name}\").");
				return null;
			}
			return BlendToCamera(target);
		}
		public void HardLookAt(Transform target) {
			if(UsesProtagonist)
				throw new System.Exception("Not supported.");
			MainCamera.transform.LookAt(target);
		}
		#endregion

		#region Functions
		private System.Collections.IEnumerator TransitCameraToCoroutine(
			Transform target,
			float duration,
			float easingFactor
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

		private System.Collections.IEnumerator BlendToCameraCoroutine(
			Camera target,
			float duration
		) {
			Debug.Log($"Start blending the camera to {target.name}.", target);
			#region Initialize
			var blendTexture = RenderUtility.CreateScreenSizedRT();
			var targetRT = target.EnsureComponent<CameraRenderingTarget>();
			target.gameObject.SetActive(true);
			var startTime = Time.time;
			#endregion

			#region Progress
			void renderingFinishingContinuation(RenderTexture result) {
				var progress = (Time.time - startTime) / duration;
				result.Overlay(targetRT.OutputTexture, progress);
			}
			mainCameraManager.onRenderingFinished += renderingFinishingContinuation;
			yield return new WaitForSeconds(duration);
			mainCameraManager.onRenderingFinished -= renderingFinishingContinuation;
			#endregion

			#region Post process
			AlignCameraTo(target.transform);
			yield return new WaitForEndOfFrame();
			#endregion

			#region Finalize
			blendTexture.Destroy();
			HierarchyUtility.Destroy(targetRT);	// Could be commented out.
			target.gameObject.SetActive(false);
			Debug.Log($"Finished camera blending.", target);
			#endregion
		}
		#endregion
	}
}