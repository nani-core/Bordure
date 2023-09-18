using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NaniCore.Loopool {
	public class Detachable : Interactable {
		#region Fields
		private List<Grabbable> disabledGrabbables = new List<Grabbable>();
		#endregion

		#region Message handlers
		protected override void OnInteract() {
			if(!isActiveAndEnabled)
				return;
			Detach();
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