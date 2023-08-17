using UnityEngine;
using System.Collections;

namespace NaniCore.UnityPlayground {
	public class Grabbable : Interaction {
		protected override void OnFocusEnter() {
		}

		protected override void OnFocusLeave() {
		}

		protected override void OnInteract() {
			Protagonist.instance.Grabbing = this;
		}

		#region Grabbing
		protected RigidbodyConstraints originalConstraints;
		protected Coroutine grabbingCoroutine;

		protected void OnGrabStart() {
			if(grabbingCoroutine != null) {
				StopCoroutine(grabbingCoroutine);
				grabbingCoroutine = null;
			}
			if(rigidbody != null) {
				originalConstraints = rigidbody.constraints;
				rigidbody.constraints = RigidbodyConstraints.FreezeAll;
			}
			grabbingCoroutine = StartCoroutine(GrabStartCoroutine());
		}

		protected void OnCollisionEnter(Collision _) {
			var protagonist = Protagonist.instance;
			if(protagonist.Grabbing == this)
				protagonist.Grabbing = null;
		}

		protected void OnGrabEnd() {
			if(grabbingCoroutine != null) {
				StopCoroutine(grabbingCoroutine);
				grabbingCoroutine = null;
			}
			transform.SetParent(null);
			if(rigidbody != null) {
				rigidbody.constraints = originalConstraints;
				grabbingCoroutine = StartCoroutine(GrabEndCoroutine());
			}
		}

		protected IEnumerator GrabStartCoroutine() {
			Protagonist protagonist = Protagonist.instance;
			transform.SetParent(protagonist.Eye);
			Vector3 startPos = transform.localPosition;
			Vector3 targetPos = Vector3.forward * protagonist.GrabbingDistance;
			float startTime = Time.time;
			float t;
			while((t = Time.time - startTime) < protagonist.GrabbingTime) {
				transform.localPosition = Vector3.Lerp(startPos, targetPos, t / protagonist.GrabbingTime);
				yield return new WaitForFixedUpdate();
			}
			transform.localPosition = targetPos;
		}

		protected IEnumerator GrabEndCoroutine() {
			if(rigidbody != null)
				yield break;
			yield return new WaitForSeconds(.1f);
			yield return new WaitUntil(() => Vector3.SqrMagnitude(rigidbody.velocity) > .01f);
		}
		#endregion
	}
}