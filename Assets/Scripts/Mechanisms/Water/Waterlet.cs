using UnityEngine;

namespace NaniCore.Stencil {
	public abstract class Waterlet : MonoBehaviour {
		#region Serialized fields
		[SerializeField] protected Water water;
		[SerializeField] protected Transform pivot;
		#endregion

		#region Interfaces
		public Water Water => water;

		public abstract bool IsSatisfied { get; }

		public void ToggleActive() {
			enabled = !enabled;
		}

		/// <summary>
		/// The relative height offset from the waterlet's pivot to the bottom of the water container.
		/// </summary>
		public float Height {
			get {
				if(water == null)
					return 0;
				var pivotRelativePosition = water.transform.worldToLocalMatrix.MultiplyPoint(pivot.position);
				return pivotRelativePosition.y;
			}
		}
		#endregion

		#region Functions
		private void OnEnabilityChanged() {
			water.UpdateTargetHeight();
			UpdateVisualState();
		}

		/// <summary>
		/// Automatically sets whether the visual cue is active.
		/// </summary>
		protected abstract void UpdateVisualState();

		/// <summary>
		/// Update the visual cue.
		/// </summary>
		protected virtual void UpdateVisualFrame() {
		}
		#endregion

		#region Message handlers
		public virtual void OnWaterHeightChange(float previousHeight) {
			UpdateVisualFrame();
			if(IsSatisfied)
				enabled = false;
		}
		#endregion

		#region Life cycle
		protected void Start() {
			water?.AddWaterlet(this);
			enabled = false;
		}

		protected void OnDestroy() {
			water?.RemoveWaterlet(this);
		}

		protected void OnEnable() {
			if(IsSatisfied)
				return;
			water.OnWaterletEnabled(this);
			OnEnabilityChanged();
		}

		protected void OnDisable() {
			OnEnabilityChanged();
		}
		#endregion
	}
}