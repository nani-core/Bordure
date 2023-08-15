using UnityEngine;
using UnityEngine.InputSystem;

namespace NaniCore.UnityPlayground {
	public class DummyWalker : MonoBehaviour {
		#region Serialized fields
		public CharacterController controller;
		public Transform eye;

		[Range(0, 5)] public float movementSpeed;
		[Range(0, 1)] public float orientationSpeed;
		public Vector2 viewingZenithRange;
		#endregion

		#region Fields
		Vector3 rawVelocity;
		float zenith;
		#endregion

		#region Input handlers
		protected void OnMove(InputValue value) {
			Vector2 raw = value.Get<Vector2>();
			rawVelocity = new Vector3(raw.x, 0, raw.y);
		}

		protected void OnOrientDelta(InputValue value) {
			Vector2 raw = value.Get<Vector2>();
			transform.rotation *= Quaternion.Euler(0, raw.x * orientationSpeed, 0);
			zenith += raw.y * orientationSpeed;
			zenith = Mathf.Clamp(zenith, viewingZenithRange.x, viewingZenithRange.y);
		}
		#endregion

		#region Life cycle
		protected void Start() {
			Cursor.lockState = CursorLockMode.Locked;
		}

		protected void FixedUpdate() {
			Vector3 velocity = transform.localToWorldMatrix * rawVelocity;
			controller.SimpleMove(velocity * movementSpeed);
			eye.localRotation = Quaternion.Euler(zenith, 0, 0);
		}
		#endregion
	}
}