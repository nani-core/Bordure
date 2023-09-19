using UnityEngine;

namespace NaniCore.Loopool {
	public abstract class Waterlet : MonoBehaviour {
		#region Serialized fields
		[SerializeField] protected Water water;
		[SerializeField] protected Transform pivot;
		[SerializeField] private bool active;
		#endregion

		#region Fields
		private bool isFlowing;
		#endregion

		#region Functions
		public bool IsActive {
			get => active;
			set {
				active = value;
				OnSetActivity();
				water?.UpdateTargetHeight();
			}
		}

		public void ToggleActive() {
			IsActive = !IsActive;
		}

		public float Height {
			get {
				if(water == null)
					return 0;
				var pivotRelativePosition = water.transform.worldToLocalMatrix.MultiplyPoint(pivot.position);
				return pivotRelativePosition.y;
			}
		}

		protected bool IsFlowing {
			get => isFlowing;
			set {
				isFlowing = value;
				UpdateVisualState();
			}
		}

		protected abstract void UpdateFlowingState();
		protected abstract void UpdateVisualState();
		protected virtual void UpdateVisualFrame() {
		}
		#endregion

		#region Message handlers
		protected virtual void OnSetActivity() {
			water.UpdateTargetHeight();
			UpdateFlowingState();
		}

		protected virtual void OnWaterHeightChange(float previousHeight) {
			float det = (Height - previousHeight) * (Height - water.Height);
			if(det <= 0)
				OnWaterHeightPass();
		}

		protected virtual void OnWaterHeightPass() {
			UpdateFlowingState();
		}
		#endregion

		#region Life cycle
		protected void Start() {
			water?.AddWaterlet(this);
			IsActive = IsActive;
		}

		protected void OnDestroy() {
			water?.RemoveWaterlet(this);
		}

		protected void FixedUpdate() {
			if(IsFlowing) {
				UpdateVisualFrame();
			}
		}
		#endregion
	}
}