using UnityEngine;
using System.Collections;

namespace NaniCore.Bordure {
	public partial class PointingCarrier : Carrier {
		#region Serialized fields
		[Tooltip("Radians per second.")]
		[SerializeField] public float speed = 1f;
		[Tooltip("The local pointing direction.")]
		[SerializeField] private Vector3 forwardDirection = new(0, 0, 1);
		#endregion

		#region Fields
		private Vector3 plannedPointingDirection;
		private Coroutine pointingCoroutine;
		#endregion

		#region Interface
		/// <summary>The local pointing direction.
		public Vector3 ForwardDirection {
			get => forwardDirection;
			set {
				forwardDirection = value;
				StartPointing();
			}
		}

		public Vector3 PointingDirection {
			get => transform.localToWorldMatrix.MultiplyVector(forwardDirection);
			set {
				plannedPointingDirection = value;
				StartPointing();
			}
		}

		public void PointToUpwardOf(Transform reference) {
			forwardDirection = transform.worldToLocalMatrix.MultiplyVector(reference.up);
			PointingDirection = Vector3.up;
		}
		#endregion

		#region Functions
		private void StartPointing() {
			if(pointingCoroutine != null) {
				StopCoroutine(pointingCoroutine);
				pointingCoroutine = null;
			}
			pointingCoroutine = StartCoroutine(PointingCoroutine());
		}

		private IEnumerator PointingCoroutine() {
			float angle = Vector3.Angle(PointingDirection, plannedPointingDirection) * Mathf.Deg2Rad;
			float totalTime = angle / speed;
			var startTime = Time.time;
			Quaternion originalQuat = transform.rotation;
			// The delta rotation to be performed.
			Quaternion deltaQuat = Quaternion.FromToRotation(PointingDirection, plannedPointingDirection);
			for(float t; (t = (Time.time - startTime) / totalTime) < 1f; ) {
				transform.rotation = Quaternion.Slerp(Quaternion.identity, deltaQuat, t) * originalQuat;
				yield return new WaitForFixedUpdate();
			}
			transform.rotation = deltaQuat * originalQuat;

			pointingCoroutine = null;
			yield break;
		}
		#endregion

		#region Life cycle
		protected new void Start() {
			base.Start();

			plannedPointingDirection = PointingDirection;
		}
		#endregion
	}
}
