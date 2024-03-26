using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore.Bordure {
	[RequireComponent(typeof(BoxCollider))]
	public partial class Water : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private Transform surface;
		[SerializeField][Min(0)] private float height = 1;
		[SerializeField][Min(0)] private float speed = 1;
		[SerializeField][NaughtyAttributes.Expandable] private WaterProfile profile;
		#endregion

		#region Fields
		private Coroutine targetHeightCoroutine;
		private readonly HashSet<Rigidbody> floatingBodies = new();
		// Could be buggy.
		private readonly HashSet<Waterlet> waterlets = new();
		#endregion

		#region Interfaces
		/// <summary>
		/// The height offset from the water surface to the bottom of the water container.
		/// </summary>
		public float Height {
			get => height;
			set {
				Vector3 center = Collider.center;
				center.y = value * .5f;
				Collider.center = center;
				Collider.size = new Vector3(1, value, 1);
				surface.localPosition = Vector3.up * value;
				var previousHeight = height;
				height = value;
				foreach(var waterlet in ActiveWaterlets)
					waterlet.OnWaterHeightChange(previousHeight);
			}
		}

		public float WorldHeight => transform.position.y + Height;

		public float TargetHeight {
			set {
				if(targetHeightCoroutine != null)
					StopCoroutine(targetHeightCoroutine);
				if(isActiveAndEnabled)
					targetHeightCoroutine = StartCoroutine(SetTargetHeightCoroutine(value));
			}
		}

		public IEnumerable<Waterlet> Waterlets => waterlets;

		public IEnumerable<Waterlet> ActiveWaterlets {
			get => waterlets.Where(waterlet => waterlet.enabled);
		}

		public bool HasAnyActiveWaterletsOtherThan(Waterlet than) {
			foreach(var waterlet in ActiveWaterlets) {
				if(waterlet != than)
					return true;
			}
			return false;
		}
		#endregion

		#region Functions
		private BoxCollider Collider => GetComponent<BoxCollider>();

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

		private void UpdateFloatingBodyPhysics(Rigidbody rigidbody, Collider collider, float deltaTime) {
			// Kinematic floatables might haven't been released yet.
			if(rigidbody.isKinematic)
				return;

			Vector3 totalForce = default, totalTorque = default;
			var bounds = collider.bounds;

			// Buoyancy force.
			{
				var sunkDepth = Height + transform.position.y - bounds.min.y;
				sunkDepth = Mathf.Clamp(sunkDepth, 0, bounds.size.y);
				var sunkVolume = bounds.size.x * bounds.size.z * sunkDepth;
				totalForce += Physics.gravity * (sunkVolume * profile.density * -1);
			}

			// Apply the effect.
			rigidbody.AddForce(totalForce, ForceMode.Force);
			rigidbody.AddTorque(totalTorque, ForceMode.Force);
		}

		private void LateUpdateFloatingBodyPhysics(Rigidbody rigidbody, Collider collider, float deltaTime) {
			// Kinematic floatables might haven't been released yet.
			if(rigidbody.isKinematic)
				return;

			Vector3 totalForce = default, totalTorque = default;
			var bounds = collider.bounds;

			// Damp.
			{

				var dampCoefficient = profile.damp * bounds.size.sqrMagnitude;
				// Resistant force is proportional to velocity squared.
				Vector3
					dampForce = rigidbody.velocity.normalized * (Mathf.Pow(rigidbody.velocity.magnitude, 2) * dampCoefficient * -1),
					dampTorque = rigidbody.angularVelocity.normalized * (Mathf.Pow(rigidbody.angularVelocity.magnitude, 2) * dampCoefficient * -1);

				totalForce += dampForce * deltaTime;
				totalTorque += dampTorque * deltaTime;
			}

			// Apply the effect.
			rigidbody.AddForce(totalForce, ForceMode.Force);
			rigidbody.AddTorque(totalTorque, ForceMode.Force);
		}

		public void AddWaterlet(Waterlet waterlet) {
			waterlets.Add(waterlet);
		}

		public void RemoveWaterlet(Waterlet waterlet) {
			waterlets.Remove(waterlet);
		}
		#endregion

		#region Message handlers
		public void OnWaterletEnabled(Waterlet target) {
			foreach(var waterlet in Waterlets) {
				if(waterlet != target && waterlet.enabled)
					waterlet.enabled = false;
			}
		}
		#endregion

		#region Life cycle
		protected void OnEnable() {
			if(profile == null) {
				Debug.LogWarning($"Warning: {this} has no water profile!", this);
				enabled = false;
				return;
			}
		}

		protected void OnTriggerEnter(Collider other) {
			if(other.transform.TryGetComponent<Rigidbody>(out var rigidbody))
				floatingBodies.Add(rigidbody);
		}
		protected void OnTriggerExit(Collider other) {
			if(other.transform.TryGetComponent<Rigidbody>(out var rigidbody))
				floatingBodies.Remove(rigidbody);
		}

		protected void FixedUpdate() {
			floatingBodies.RemoveWhere(f => f == null);
			foreach(var body in floatingBodies) {
				var collider = body.GetComponent<Collider>();
				UpdateFloatingBodyPhysics(body, collider, Time.fixedDeltaTime);
				LateUpdateFloatingBodyPhysics(body, collider, Time.fixedDeltaTime);
			}
		}
		#endregion
	}
}