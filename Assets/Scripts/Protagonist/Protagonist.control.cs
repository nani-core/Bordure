using UnityEngine;

namespace NaniCore.Loopool {
	public partial class Protagonist : MonoBehaviour {
		#region Serialized fields
		[Header("Geometry")]
		[SerializeField][Min(0)] private float height = 1.6f;
		[SerializeField][Min(0)] private float radius = .3f;
		[SerializeField] private Transform eye;
		[SerializeField][Min(0)] private float eyeHanging = .1f;

		[Header("Control")]
		[SerializeField][Min(0)] private float skinDepth = .08f;
		[SerializeField][Min(0)] private float walkingSpeed = 3f;
		[SerializeField][Min(0)] private float sprintingSpeed = 5f;
		[SerializeField][Min(0)] private float stepDistance = 1.3f;
		[SerializeField][Min(0)] private float orientingSpeed = 1f;
		[SerializeField][Min(0)] private float jumpingHeight = 1f;
		#endregion

		#region Fields
		private CapsuleCollider capsuleCollider;
		private new Rigidbody rigidbody;
		private bool isOnGround = false;
		private bool isRunning = false;
		private float steppedDistance = 0;
		#endregion

		#region Properties
		public Vector3 Upward => transform.up;

		public bool IsOnGround => isOnGround;

		public bool IsSprinting {
			get => isRunning;
			set => isRunning = value;
		}

		private float MovingSpeed => IsSprinting ? sprintingSpeed : walkingSpeed;

#pragma warning disable IDE0052 // Remove unread private members
		private float SteppedDistance {
			get => steppedDistance;
			set {
				steppedDistance = value;
				if(stepDistance <= 0)
					return;
				if(steppedDistance < 0 || steppedDistance > stepDistance) {
					steppedDistance = steppedDistance.Mod(stepDistance);
					PlayFootstepSound();
				}
			}
		}
#pragma warning restore IDE0052 // Remove unread private members

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

		private void InitializeControl() {
			capsuleCollider = gameObject.EnsureComponent<CapsuleCollider>();
			capsuleCollider.height = height;
			capsuleCollider.center = Vector3.up * (height * .5f);
			capsuleCollider.radius = radius;

			rigidbody = gameObject.EnsureComponent<Rigidbody>();
			rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

			if(eye != null) {
				eye.localPosition = Vector3.up * (height - eyeHanging);
			}
		}
		#endregion

		#region Life cycle
#if UNITY_EDITOR
		private void ValidateControl() {
			InitializeControl();
		}
#endif

		protected void StartControl() {
			InitializeControl();
		}

		protected void UpdateControl() {
			isOnGround = SweepTest(Physics.gravity, out _, skinDepth, .5f);
		}
		#endregion

		#region Functions
		private bool SweepTest(Vector3 direction, out RaycastHit hitInfo, float distance, float backupRatio = 0) {
			direction = direction.normalized;
			var originalPos = rigidbody.position;
			rigidbody.position -= direction * distance * backupRatio;
			bool result = rigidbody.SweepTest(direction, out hitInfo, distance);
			rigidbody.position = originalPos;
			return result;
		}

		public void OrientDelta(Vector2 delta) {
			delta *= orientingSpeed;
			Azimuth += delta.x;
			Zenith += delta.y;
		}

		public void MoveVelocity(Vector2 vXy) {
			if(!IsOnGround)
				return;
			vXy *= MovingSpeed;
			var horizontalVelocity = (eye.transform.right * vXy.x + transform.forward * vXy.y).ProjectOntoPlane(Upward);
			var verticalVelocity = rigidbody.velocity.ProjectOntoAxis(Upward);
			rigidbody.velocity = horizontalVelocity + verticalVelocity;
		}

		public void MoveDelta(Vector3 delta) {
			if(!IsOnGround)
				return;
			rigidbody.MovePosition(rigidbody.position + delta);
			SteppedDistance += delta.magnitude;
		}

		public void Jump() {
			if(!IsOnGround)
				return;
			var gravity = -Vector3.Dot(Physics.gravity, Upward);
			float speed = Mathf.Sqrt(2f * gravity * jumpingHeight);
			rigidbody.AddForce(Upward * speed, ForceMode.VelocityChange);
		}
		#endregion
	}
}