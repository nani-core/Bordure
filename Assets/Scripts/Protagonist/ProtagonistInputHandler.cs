using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI.Table;

namespace NaniCore.Bordure {
	[RequireComponent(typeof(Protagonist))]
	public class ProtagonistInputHandler : MonoBehaviour {
		#region Fields
		private Protagonist protagonist;
		[SerializeField] private PlayerInput playerInput;
		private InputActionMap orientationMap;

		private Vector3 moveVelocity;
		private float floating = 0f, sinking = 0f;

		private bool grabbingOrienting = false;
		private bool grabbingDistancing = false;
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

		private InputActionMap OrientationMap {
			get {
				if(orientationMap != null)
					return orientationMap;
				return orientationMap = playerInput.actions.FindActionMap("Orientation");
			}
		}

		private bool ShouldOrientationBeEnabled => !(GrabbingOrienting || GrabbingDistancing);

		private bool OrientationMapEnabled {
			get => OrientationMap.enabled;
			set {
				if(value)
					OrientationMap.Enable();
				else
					OrientationMap.Disable();
			}
		}

		private bool GrabbingOrienting {
			get => grabbingOrienting;
			set {
				grabbingOrienting = value;
				OrientationMapEnabled = ShouldOrientationBeEnabled;
			}
		}

		private bool GrabbingDistancing {
			get => grabbingDistancing;
			set {
				grabbingDistancing = value;
				OrientationMapEnabled = ShouldOrientationBeEnabled;
			}
		}
		#endregion

		#region Handlers
		// Normal

		protected void OnInteract() {
			protagonist.Interact();
		}

		protected void OnCheat() => protagonist?.Cheat();

		protected void OnPause() {
			Game.Ui.OpenStartMenu();
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
			if(!OrientationMapEnabled || !protagonist.UsesOrientation)
				return;

			Vector2 raw = value.Get<Vector2>();
			raw *= Game.mouseSensitivityGain;
			protagonist.OrientDelta(raw);
		}

		// Grabbing

		protected void OnSetGrabbingOrienting(InputValue value) {
			bool raw = value.Get<float>() > .5f;
			GrabbingOrienting = raw;
		}

		protected void OnSetGrabbingDistancing(InputValue value) {
			bool raw = value.Get<float>() > .5f;
			GrabbingDistancing = raw;
		}

		protected void OnGrabbingDistanceDelta(InputValue value) {
			if(protagonist.GrabbingObject == null || !GrabbingDistancing)
				return;

			float raw = value.Get<float>();
			raw *= protagonist.Profile.grabbingDistanceScrollingSpeed;
			protagonist.GrabbingDistance += raw;
		}

		protected void OnGrabbingOrientDelta(InputValue value) {
			if(protagonist.GrabbingObject == null || !GrabbingOrienting)
				return;

			Vector2 raw = value.Get<Vector2>();
			raw *= Game.mouseSensitivityGain;
			protagonist.GrabbingOrientDelta(raw);
		}
		#endregion
	}
}