using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore.Stencil {
	[RequireComponent(typeof(BoxCollider))]
	public partial class Water : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private Transform surface;
		[SerializeField][Min(0)] private float height = 1;
		[SerializeField][Min(0)] private float speed = 1;
		[SerializeField] private WaterProfile profile;
		#endregion

		#region Fields
		private Coroutine targetHeightCoroutine;
		private readonly HashSet<Rigidbody> floatingBodies = new();
		// Could be buggy.
		private HashSet<Waterlet> waterlets = new();
		#endregion

		#region Interfaces
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

		private void UpdateFloatingBodyPhysicsOnEndOfFixedUpdate(Rigidbody rigidbody) {
			// Kinematic floatables might haven't been released yet.
			if(rigidbody.isKinematic)
				return;

			var downward = Physics.gravity.normalized;
			// Positive is downward.
			var offsetToSurface = Vector3.Dot(downward, rigidbody.position - transform.position) + Height;
			var buoyancy = downward * -Mathf.Clamp(offsetToSurface, 0, 1);
			var friction = -rigidbody.velocity * profile.damp;
			friction = downward * Vector3.Dot(downward, friction);
			// TODO: make this time-independent.
			rigidbody.AddForce(buoyancy + friction, ForceMode.Impulse);
		}

		public void AddWaterlet(Waterlet waterlet) {
			if(waterlets == null)
				waterlets = new HashSet<Waterlet>();
			waterlets.Add(waterlet);
		}

		public void RemoveWaterlet(Waterlet waterlet) {
			waterlets.Remove(waterlet);
		}

		public void UpdateTargetHeight() {
			var activeWaterlets = ActiveWaterlets;
			List<Waterlet> pumps = new List<Waterlet>(), dumps = new List<Waterlet>();
			foreach(var waterlet in activeWaterlets) {
				if(waterlet is WaterPump)
					pumps.Add(waterlet);
				if(waterlet is WaterDump)
					dumps.Add(waterlet);
			}
			float height = Height;
			foreach(var dump in dumps)
				height = Mathf.Min(height, dump.Height);
			foreach(var pump in pumps)
				height = Mathf.Max(height, pump.Height);
			TargetHeight = height;
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
			var rigidbody = other.transform.GetComponent<Rigidbody>();
			if(rigidbody != null)
				floatingBodies.Add(rigidbody);
		}
		protected void OnTriggerExit(Collider other) {
			var rigidbody = other.transform.GetComponent<Rigidbody>();
			if(rigidbody != null)
				floatingBodies.Remove(rigidbody);
		}

		protected void FixedUpdate() {
			floatingBodies.RemoveWhere(f => f == null);
			foreach(var floatable in floatingBodies) {
				UpdateFloatingBodyPhysicsOnEndOfFixedUpdate(floatable);
			}
		}
		#endregion
	}
}