using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace NaniCore.Stencil {
	[RequireComponent(typeof(Rigidbody))]
	public partial class Detachable : MonoBehaviour {
		#region Serialized fields
		public Vector3 ejectionVelocity;
		public Vector3 ejectionOrigin;
		public UnityEvent onDetached;
		#endregion

		#region Fields
		private readonly List<Grabbable> disabledGrabbables = new List<Grabbable>();
		private new Rigidbody rigidbody;
		#endregion

		#region Message handlers
		protected void OnDetached() {
			onDetached?.Invoke();
		}
		#endregion

		#region Functions
		private Rigidbody Rigidbody {
			get {
				if(rigidbody == null)
					rigidbody = GetComponent<Rigidbody>();
				return rigidbody;
			}
		}

		public void Detach() {
			StartCoroutine(DetachCoroutine());
		}

		private IEnumerator DetachCoroutine() {
			transform.SetParent(null, true);
			var meshCollider = Rigidbody.GetComponent<MeshCollider>();
			if(meshCollider != null) {
				meshCollider.convex = true;
			}
			Rigidbody.isKinematic = false;
			Rigidbody.AddForceAtPosition(
				transform.localToWorldMatrix.MultiplyVector(ejectionVelocity).normalized * ejectionVelocity.magnitude,
				transform.localToWorldMatrix.MultiplyPoint(ejectionOrigin),
				ForceMode.VelocityChange
			);

			yield return new WaitForEndOfFrame();

			foreach(var grabbable in disabledGrabbables)
				grabbable.enabled = true;
			disabledGrabbables.Clear();

			SendMessage("OnDetached", SendMessageOptions.DontRequireReceiver);
			enabled = false;
		}
		#endregion
	}
}