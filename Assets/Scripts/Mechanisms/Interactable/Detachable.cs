using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;

namespace NaniCore.Loopool {
	public partial class Detachable : Interactable {
		#region Serialized fields
		public bool useDetachingEjection;
		[ShowIf("useDetachingEjection")] public Vector3 ejectionVelocity;
		[ShowIf("useDetachingEjection")] public Vector3 ejectionOrigin;
		public UnityEvent onDetached;
		#endregion

		#region Fields
		private List<Grabbable> disabledGrabbables = new List<Grabbable>();
		#endregion

		#region Message handlers
		protected override void OnInteract() {
			if(!isActiveAndEnabled)
				return;
			Detach();
		}

		protected void OnDetached() {
			onDetached?.Invoke();
		}
		#endregion

		#region Functions
		public void Detach() {
			StartCoroutine(DetachCoroutine());
		}

		private IEnumerator DetachCoroutine() {
			transform.SetParent(null, true);
			if(Rigidbody) {
				Rigidbody.isKinematic = false;
				if(useDetachingEjection) {
					Rigidbody.AddForceAtPosition(
						transform.localToWorldMatrix.MultiplyVector(ejectionVelocity).normalized * ejectionVelocity.magnitude,
						transform.localToWorldMatrix.MultiplyPoint(ejectionOrigin),
						ForceMode.VelocityChange
					);
				}
			}

			yield return new WaitForEndOfFrame();

			foreach(var grabbable in disabledGrabbables)
				grabbable.enabled = true;
			disabledGrabbables.Clear();

			SendMessage("OnDetached", SendMessageOptions.DontRequireReceiver);
			enabled = false;
		}
		#endregion

		#region Life cycle
		protected new void Start() {
			if(Rigidbody) {
				Rigidbody.isKinematic = true;
			}
			disabledGrabbables.AddRange(transform.GetComponentsInChildren<Grabbable>(false));
			foreach(var grabbable in disabledGrabbables)
				grabbable.enabled = false;
		}
		#endregion
	}
}