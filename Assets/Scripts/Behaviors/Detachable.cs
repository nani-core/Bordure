using UnityEngine;
using UnityEngine.Events;

namespace NaniCore.Bordure {
	[RequireComponent(typeof(Rigidbody))]
	public partial class Detachable : MonoBehaviour {
		#region Serialized fields
		public Vector3 ejectionVelocity;
		public Vector3 ejectionOrigin;
		public UnityEvent onDetached;
		#endregion

		#region Fields
		private new Rigidbody rigidbody;
		#endregion

		#region Message handlers
		protected void OnDetached() {
			onDetached?.Invoke();
		}
		#endregion

		#region Interfaces
		public void Detach() {
			if(rigidbody == null)
				rigidbody = GetComponent<Rigidbody>();

			if(rigidbody.TryGetComponent<MeshCollider>(out var meshCollider)) {
				var mesh = meshCollider.sharedMesh;
				if(!(mesh?.isReadable ?? false)) {
					Debug.LogWarning($"Warning: {mesh} must be set to accessible in order to create a convex hull.", mesh);
				}
				meshCollider.convex = true;
			}

			Transform parent = transform.parent;
			while(!parent.gameObject.isStatic) {
				if(parent.parent == null)
					break;
				parent = parent.parent;
			}
			transform.SetParent(parent, true);

			rigidbody.isKinematic = false;
			rigidbody.AddForceAtPosition(
				transform.localToWorldMatrix.MultiplyVector(ejectionVelocity).normalized * ejectionVelocity.magnitude,
				transform.localToWorldMatrix.MultiplyPoint(ejectionOrigin),
				ForceMode.VelocityChange
			);

			SendMessage("OnDetached", SendMessageOptions.DontRequireReceiver);
			enabled = false;
		}
		#endregion
	}
}