using UnityEngine;

namespace NaniCore.UnityPlayground {
	public class Waterlet : MonoBehaviour {
		#region Serialized fields
		[SerializeField] protected Water water;
		[SerializeField] protected Transform pivot;
		[SerializeField] private bool active;
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
		#endregion

		#region Message handlers
		protected virtual void OnSetActivity() {
		}

		protected virtual void OnWaterLevelChange(float previousHeight) {
			float det = (Height - previousHeight) * (Height - water.Height);
			if(det < 0)
				OnWaterLevelPass();
		}

		protected virtual void OnWaterLevelPass() {
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
		#endregion
	}
}