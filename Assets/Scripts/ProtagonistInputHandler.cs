using UnityEngine;
using UnityEngine.InputSystem;

namespace NaniCore.UnityPlayground {
	[RequireComponent(typeof(Protagonist))]
	[RequireComponent(typeof(PlayerInput))]
	public class ProtagonistInputHandler : MonoBehaviour {
		#region Fields
		protected Protagonist protagonist;
		#endregion

		#region Life cycle
		protected void Start() {
			protagonist = GetComponent<Protagonist>();
		}

		protected void OnEnable() {
			Cursor.lockState = CursorLockMode.Locked;
		}

		protected void OnDisable() {
			Cursor.lockState = CursorLockMode.None;
		}
		#endregion

		#region Internal accessors
		protected Protagonist Protagonist => protagonist;
		#endregion

		#region Handlers
		protected void OnOrientDelta(InputValue value) {
			Vector2 raw = value.Get<Vector2>();
			Protagonist.Azimuth += raw.x;
			Protagonist.Zenith += raw.y;
		}
		#endregion
	}
}