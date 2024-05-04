using UnityEngine;
using UnityEngine.InputSystem;

namespace NaniCore.Bordure {
	[RequireComponent(typeof(Protagonist))]
	public class ProtagonistInputHandler : MonoBehaviour {
		#region Fields
		private Protagonist protagonist;
		[SerializeField] private PlayerInput playerInput;

		private Vector3 moveVelocity;
		private float floating = 0f, sinking = 0f;
		#endregion

		#region Life cycle
		protected void Start() {
			protagonist = GetComponent<Protagonist>();

			foreach(var map in playerInput.actions.actionMaps)
				map.Enable();
		}

		protected void FixedUpdate() {
			if(protagonist.UsesMovement) {
				if(!protagonist.IsInWater)
					moveVelocity.y = 0;
				else
					moveVelocity.y = floating - sinking;

				protagonist.MoveVelocity(moveVelocity);
			}
		}

		protected void OnEnable() {
			playerInput.enabled = true;
		}

		protected void OnDisable() {
			playerInput.enabled = false;
		}
		#endregion

		#region Functions
		private GameManager Game => GameManager.Instance;
		#endregion

		#region Handlers
		// Normal

		protected void OnInteract() {
			protagonist.Interact();
		}

		protected void OnCheat() => protagonist?.Cheat();

		protected void OnPause() {
			Game.SettingsUiIsOpen = true;
		}

		// Movement

		protected void OnMoveVelocity(InputValue value) {
			var raw = value.Get<Vector2>();
			if(Game.CurrentSeat == null) {
				// Normal movement
				moveVelocity.x = raw.x;
				moveVelocity.z = raw.y;
			}
			else {
				// Leave seat
				if(raw.magnitude > 0.5f && Game.CurrentSeat.canLeaveManually)
					Game.ProtagonistLeaveSeat();
			}
		}

		protected void OnSetSprinting(InputValue value) {
			protagonist.IsSprinting = value.Get<float>() > .5f;
		}

		protected void OnJump(InputValue value) {
			float raw = value.Get<float>();
			floating = raw;
			if(raw > 0) {
				// Can jump in water.
				protagonist.Jump();
			}
		}

		protected void OnCrouch(InputValue value) {
			float raw = value.Get<float>();
			sinking = raw;
		}

		// Orientation

		protected void OnOrientDelta(InputValue value) {
			Vector2 raw = value.Get<Vector2>();
			raw *= Game.mouseSensitivityGain;
			if(!protagonist.GrabbingOrienting) {
				if(protagonist.UsesOrientation)
					protagonist.OrientDelta(raw);
			}
			else {
				protagonist.GrabbingOrientDelta(-raw.x);
			}
		}

		// Grabbing

		protected void OnSetGrabbingOrienting(InputValue value) {
			bool raw = value.Get<float>() > .5f;
			protagonist.GrabbingOrienting = raw;
		}

		protected void OnResetGrabbingTransform() {
			protagonist.ResetGrabbingTransform();
		}
		#endregion
	}
}