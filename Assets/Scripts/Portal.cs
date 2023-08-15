using UnityEngine;

namespace NaniCore.UnityPlayground {
	/// <summary>
	/// This class represents a single side of a portal.
	/// </summary>
	/// <remarks>
	///	This class is implemented partially in several files.
	/// </remarks>
	public partial class Portal : MonoBehaviour {
		#region Serialized fields
		public Portal twin;
		#endregion

		#region Fields
		#endregion

		#region Functions
		#endregion

		#region Life cycle
		protected void Awake() {
			projectiveTransformShader = Shader.Find(projectiveTransformShaderName);
			projectiveTransformMaterial = new Material(projectiveTransformShader);

			viewCameraLayerMask = ~0 & ~LayerMask.GetMask(portalLayerName);
		}

		protected void Start() {
			InitializeRendering();
		}

		protected void OnDestroy() {
			FinalizeRendering();
		}

		protected void Update() {
			UpdateRendering();
		}

		protected void LateUpdate() {
			LateUpdateRendering();
			LastUpdatePassenger();
		}

		protected void OnBecameVisible() => enabled = true;
		protected void OnBecameInvisible() => enabled = false;

		protected void OnTriggerEnter(Collider other) {
			OnColliderEnterPortal(other);
		}

		protected void OnTriggerExit(Collider other) {
			OnColliderExitPortal(other);
		}
		#endregion
	}
}