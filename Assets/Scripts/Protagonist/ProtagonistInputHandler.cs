using UnityEngine;
using UnityEngine.InputSystem;

namespace NaniCore.Bordure {
	[RequireComponent(typeof(Protagonist))]
	public class ProtagonistInputHandler : MonoBehaviour {
		#region Fields
		protected Protagonist protagonist;
		protected PlayerInput playerInput;
		private InputActionMap grabbingActionMap;

		protected Vector3 moveVelocity;
		protected float floating = 0f, sinking = 0f;
		#endregion

		#region Life cycle
		protected void Start() {
			protagonist = GetComponent<Protagonist>();

			playerInput = gameObject.EnsureComponent<PlayerInput>();
			playerInput.notificationBehavior = PlayerNotifications.SendMessages;
			playerInput.actions.FindActionMap("Normal").Enable();

			grabbingActionMap = playerInput.actions.FindActionMap("Grabbing");
		}

		protected void OnEnable() {
			Cursor.lockState = CursorLockMode.Locked;
		}

		protected void OnDisable() {
			Cursor.lockState = CursorLockMode.None;
		}

		protected void FixedUpdate() {
			if(!protagonist.IsInWater)
				moveVelocity.y = 0;
			else
				moveVelocity.y = floating - sinking;

			protagonist.MoveVelocity(moveVelocity);
		}
		#endregion

		#region Functions
		public void SetGrabbingActionEnabled(bool enabled) {
			if(enabled)
				grabbingActionMap.Enable();
			else
				grabbingActionMap.Disable();
		}
		#endregion

		#region Handlers
		protected void OnMoveVelocity(InputValue value) {
			var raw = value.Get<Vector2>();
			moveVelocity.x = raw.x;
			moveVelocity.z = raw.y;
		}

		protected void OnOrientDelta(InputValue value) {
			Vector2 raw = value.Get<Vector2>();
			if(!protagonist.GrabbingOrienting)
				protagonist.OrientDelta(raw);
			else
				protagonist.GrabbingOrientDelta(-raw.x);
		}

		protected void OnSetSprinting(InputValue value) {
			protagonist.IsSprinting = value.Get<float>() > .5f;
		}

		protected void OnJump(InputValue value) {
			float raw = value.Get<float>();
			floating = raw;
			if(raw > 0) {
				// Can jump in water.
				if(protagonist.IsOnGround)
					protagonist.Jump();
			}
		}

		protected void OnCrouch(InputValue value) {
			float raw = value.Get<float>();
			sinking = raw;
		}

		protected void OnInteract() {
			protagonist.Interact();
		}

		protected void OnCheat() => protagonist?.Cheat();

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