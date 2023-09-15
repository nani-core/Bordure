using UnityEngine;

namespace NaniCore.Loopool {
	[RequireComponent(typeof(CharacterController))]
	public partial class Protagonist : MonoBehaviour {
		#region Serialized fields
		[Header("Geometry")]
		[SerializeField][Min(0)] protected float height;
		[SerializeField][Min(0)] protected float radius;
		[SerializeField] protected Transform eye;
		[SerializeField][Min(0)] protected float eyeHanging;

		[Header("Physics")]
		[SerializeField] protected CharacterController characterController;

		[Header("Control")]
		[SerializeField][Min(0)] protected float walkingSpeed;
		[SerializeField][Min(0)] protected float sprintingSpeed;
		[SerializeField][Min(0)] protected float orientingSpeed;
		#endregion

		#region Fields
		protected bool isRunning = false;
		#endregion

		#region Life cycle
#if UNITY_EDITOR
		protected void ValidateControl() {
			characterController = GetComponent<CharacterController>();
			if(characterController != null) {
				characterController.height = height;
				characterController.center = Vector3.up * (height * .5f);
				characterController.radius = radius;
			}

			if(eye != null) {
				eye.localPosition = Vector3.up * (height - eyeHanging);
			}
		}
#endif
		#endregion

		#region Functions
		public Transform Eye => eye;

		public bool IsSprinting {
			get => isRunning;
			set => isRunning = value;
		}

		protected float MovingSpeed => IsSprinting ? sprintingSpeed : walkingSpeed;

		/// <summary>
		/// What direction is the protagonist looking at, in rad.
		/// </summary>
		/// <remarks>
		/// Z+ is the starting position, rotating clockwise.
		/// </remarks>
		public float Azimuth {
			get {
				Vector3 forward = transform.forward;
				return Mathf.Atan2(forward.x, forward.z);
			}
			set {
				float degree = value * 180 / Mathf.PI;
				// Needs to be refined to support unusual cases.
				transform.rotation = Quaternion.Euler(0, degree, 0);
			}
		}

		/// <summary>
		/// How high is the protagonist looking at, in rad.
		/// </summary>
		/// <remarks>
		/// Above the XZ plane is positive; below is negative.
		/// </remarks>
		public float Zenith {
			get {
				Vector3 forward = eye.forward;
				float y = forward.y;
				forward.y = 0;
				return Mathf.Atan2(y, forward.magnitude);
			}
			set {
				float degree = value * 180 / Mathf.PI;
				degree = Mathf.Clamp(degree, -90, 90);
				eye.localRotation = Quaternion.Euler(-degree, 0, 0);
			}
		}

		public void OrientDelta(Vector2 delta) {
			delta *= orientingSpeed;
			Azimuth += delta.x;
			Zenith += delta.y;
		}

		public void MoveVelocity(Vector2 vXy) {
			vXy *= MovingSpeed;
			Vector3 v = eye.transform.right * vXy.x + transform.forward * vXy.y;
			characterController.SimpleMove(v);
		}
		#endregion
	}
}