using UnityEngine;

namespace NaniCore.Loopool {
	[RequireComponent(typeof(RigidbodyAgent))]
	public partial class Protagonist : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private new Camera camera;
		[SerializeField] private Animator animator;
		[SerializeField] private Transform eye;
		#endregion

		#region Fields
		private CapsuleCollider capsuleCollider;
		private new Rigidbody rigidbody;
		private RigidbodyAgent rigidbodyAgent;
		private bool isOnGround = false;
		private bool hasJustMoved = false;
		private bool isWalking = false;
		private bool isSprinting = false;
		private bool isInWater = false;
		/// <summary>The 2D movement input during this frame, in operation space.</summary>
		private Vector3 bufferedMovement;
		/// <summary>The target velocity due to movement, in world space.</summary>
		private Vector3 desiredHorizontalMovement;
		#endregion

		#region Properties
		public Transform Eye => eye;
		public Camera Camera => camera;
		public Vector3 Upward => transform.up;

		public bool IsInWater => isInWater;
		public bool IsOnGround => isOnGround;
		/// <summary>True when moving on ground.</summary>
		public bool IsWalking => isWalking;
		public bool IsSprinting {
			get => isSprinting;
			set => isSprinting = value;
		}

		private float WalkingSpeed => IsSprinting ? Profile.sprintingSpeed : Profile.walkingSpeed;

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
		#endregion

		#region Life cycle
		protected void InitializeControl() {
			if(Profile == null)
				return;

			ApplyGeometry();

			if(eye == null) {
				eye = transform.Find("Eye") ?? new GameObject("Eye").transform;
				eye.SetParent(transform, false);
			}
			eye.localPosition = Vector3.up * (Profile.height - Profile.eyeHanging);
			eye.localRotation = Quaternion.identity;
			eye.localScale = Vector3.one;

			rigidbodyAgent = GetComponent<RigidbodyAgent>();
		}

#if UNITY_EDITOR
		protected void ValidateControl() {
			ApplyGeometry();
		}
#endif

		protected void FixedUpdateControl() {
			ValidateMovementConditions();

			if(!IsInWater) {
				// Walking.

				desiredHorizontalMovement = eye.transform.right * bufferedMovement.x + transform.forward * bufferedMovement.z;
				desiredHorizontalMovement *= WalkingSpeed;
				if(!IsOnGround)
					desiredHorizontalMovement *= Profile.midAirAttenuation;

				var deltaVelocity = (desiredHorizontalMovement - rigidbody.velocity) * Profile.acceleration;
				// Only taking horizontal movement into account.
				deltaVelocity = deltaVelocity.ProjectOntoPlane(Upward);
				rigidbody.AddForce(deltaVelocity, ForceMode.VelocityChange);
			}
			else {
				// Swimming.

				// Extract vertical movement.
				var verticalMovement = bufferedMovement.y;
				bufferedMovement.y = 0;

				// Apply horizontal movement.
				desiredHorizontalMovement = eye.transform.localToWorldMatrix.MultiplyVector(bufferedMovement) * Profile.swimmingSpeed;
				Vector3 deltaVelovity = desiredHorizontalMovement - rigidbody.velocity;
				if(desiredHorizontalMovement.magnitude > .1f)
					deltaVelovity = deltaVelovity.ProjectOntoAxis(desiredHorizontalMovement);
				else
					deltaVelovity = -rigidbody.velocity.ProjectOntoPlane(Upward);
				rigidbody.AddForce(deltaVelovity * Profile.acceleration, ForceMode.VelocityChange);

				// Apply vertical movement.
				if(verticalMovement > .1f) {
					float verticalForce = verticalMovement * Profile.swimmingSpeed - Vector3.Dot(rigidbody.velocity, Upward);
					rigidbody.AddForce(Upward * (verticalForce * Profile.acceleration), ForceMode.VelocityChange);
				}

				// Apply friction.
				rigidbody.AddForce(-rigidbody.velocity * .08f, ForceMode.VelocityChange);
				// Apply buoyancy.
				rigidbody.AddForce(-Physics.gravity * .3f * rigidbody.mass, ForceMode.Force);
			}

			if(IsOnGround && hasJustMoved && !IsInWater)
				DealStepping(Time.fixedDeltaTime);

			UpdateMovingAnimation();

			bufferedMovement = Vector3.zero;
		}
		#endregion

		#region Functions
		private bool SweepTestGround(out RaycastHit hit, float distance, float backUpRatio = .5f, Vector3 offset = default) {
			var direction = -Upward;
			var hits = rigidbody.SweepTestAll(direction, distance, backUpRatio, offset, GameManager.Instance.GroundLayerMask);

			foreach(var candidate in hits) {
				// Check if the step is too sloped.
				float maxGroundingAngle = Mathf.Deg2Rad * Profile.maxGroundingAngle;
				float angle = Mathf.Deg2Rad * Vector3.Angle(-direction, candidate.normal);
				if(angle > maxGroundingAngle)
					continue;

				hit = candidate;
				return true;
			}

			hit = default;
			return false;
		}

		private void ApplyGeometry() {
			if(Profile == null)
				return;

			capsuleCollider = gameObject.EnsureComponent<CapsuleCollider>();
			capsuleCollider.height = Profile.height;
			capsuleCollider.center = Vector3.up * (Profile.height * .5f);
			capsuleCollider.radius = Profile.radius;

			rigidbody = gameObject.EnsureComponent<Rigidbody>();
			rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
		}

		public void OrientDelta(Vector2 delta) {
			delta *= Profile.orientingSpeed;
			Azimuth += delta.x;
			Zenith += delta.y;
		}

		public void MoveVelocity(Vector3 velocity) {
			bufferedMovement += velocity;
			hasJustMoved = hasJustMoved || velocity.magnitude > .01f;
		}

		public void Jump(float height) {
			if(!IsOnGround)
				return;

			var gravity = -Vector3.Dot(Physics.gravity, Upward);
			float desiredSpeed = Mathf.Sqrt(2f * gravity * height);
			float actualSpeed = Vector3.Dot(rigidbody.velocity, Upward);
			float deltaSpeed = desiredSpeed - actualSpeed;

			rigidbody.AddForce(Upward * deltaSpeed, ForceMode.VelocityChange);
		}
		public void Jump() => Jump(Profile.jumpingHeight);

		private void ValidateMovementConditions() {
			isOnGround = SweepTestGround(out RaycastHit _, Profile.skinDepth);
			isInWater = rigidbodyAgent.IsOverlappingWithLayers(1 << GameManager.Instance.WaterLayer);
		}

		private void DealStepping(float deltaTime) {
			// Calculate the step horizontal offset.
			// Don't use real horizontal velocity, or the cast won't succeed when touching walls.
			Vector3 vx = desiredHorizontalMovement.ProjectOntoPlane(Upward);
			Vector3 deltaX = vx * Mathf.Max(deltaTime, .1f);
			// For greater moving speed, the offset is amplified to hitting the staircase.
			if(deltaX.magnitude < Profile.stepDetectionDistance)
				deltaX = deltaX.normalized * Profile.stepDetectionDistance;

			// Perform the sweep cast and calculate vertical offset.
			var isHit = SweepTestGround(out RaycastHit hit, Profile.stepHeight * 2, .5f, deltaX);
			if(!isHit)
				return;
			float deltaY = Vector3.Dot(hit.point - rigidbody.position, Upward);

			// Don't do downward steppings.
			if(deltaY < 1e-1f)
				return;
			Jump(deltaY);
		}

		private void UpdateMovingAnimation() {
			isWalking = !IsInWater && IsOnGround && hasJustMoved;
			hasJustMoved = false;
			animator?.SetBool("Walking", isWalking);
			animator?.SetBool("Sprinting", IsSprinting);
		}
		#endregion
	}
}