using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NaniCore.UnityPlayground {
	[RequireComponent(typeof(BoxCollider))]
	public partial class Water : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private Transform surface;
		[SerializeField][Min(0)] private float height = 1;
		[SerializeField][Min(0)] private float speed = 1;
		[SerializeField][Range(0, 1)] private float resistance = .5f;
		#endregion

		#region Fields
		private Coroutine targetHeightCoroutine;
		private HashSet<Floatable> floatables = new HashSet<Floatable>();
		#endregion

		#region Functions
		private BoxCollider Collider => GetComponent<BoxCollider>();

		private float Height {
			get => height;
			set {
				Vector3 center = Collider.center;
				center.y = value * .5f;
				Collider.center = center;
				Collider.size = new Vector3(1, value, 1);
				surface.localPosition = Vector3.up * value;
				height = value;
			}
		}

		public float TargetHeight {
			set {
				if(targetHeightCoroutine != null)
					StopCoroutine(targetHeightCoroutine);
				targetHeightCoroutine = StartCoroutine(SetTargetHeightCoroutine(value));
			}
		}

		private IEnumerator SetTargetHeightCoroutine(float value) {
			if(Height == value)
				yield break;
			float height = Height;
			float sgn = Mathf.Sign(value - height);
			while(true) {
				height += sgn * speed * Time.fixedDeltaTime;
				if(Mathf.Sign(value - height) * sgn <= 0)
					break;
				Height = height;
				yield return new WaitForFixedUpdate();
			}
			Height = value;
			targetHeightCoroutine = null;
		}

		private void UpdateFloatablePhysicsOnEndOfFixedUpdate(Floatable floatable) {
			var rb = floatable.Rigidbody;
			var downward = Physics.gravity.normalized;
			// Positive is downward.
			var offsetToSurface = Vector3.Dot(downward, rb.position - transform.position) + height;
			var buoyancy = downward * -Mathf.Clamp(offsetToSurface, 0, 1);
			var friction = -rb.velocity * resistance;
			friction = downward * Vector3.Dot(downward, friction);
			// TODO: make this time-independent.
			rb.velocity += buoyancy + friction;
		}
		#endregion

		#region Life cycle
		protected void OnTriggerEnter(Collider other) {
			var floatable = other.transform.GetComponent<Floatable>();
			if(floatable != null)
				floatables.Add(floatable);
		}
		protected void OnTriggerExit(Collider other) {
			var floatable = other.transform.GetComponent<Floatable>();
			if(floatable != null)
				floatables.Remove(floatable);
		}

		protected void FixedUpdate() {
			floatables.RemoveWhere(f => f == null);
			foreach(var floatable in floatables) {
				if(!floatable.isActiveAndEnabled)
					continue;
				UpdateFloatablePhysicsOnEndOfFixedUpdate(floatable);
			}
		}
		#endregion
	}
}