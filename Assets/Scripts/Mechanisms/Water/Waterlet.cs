using UnityEngine;

namespace NaniCore.Bordure {
	public abstract class Waterlet : MonoBehaviour {
		#region Serialized fields
		[SerializeField] protected WaterBody water;
		[SerializeField] protected Transform pivot;
		#endregion

		#region Interfaces
		public WaterBody Water => water;

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

		#region Message handlers
		public virtual void OnWaterHeightChange(float previousHeight) {
			if(IsSatisfied)
				enabled = false;
		}
		#endregion

		#region Life cycle
		protected void OnEnable() {
			water?.AddWaterlet(this);
		}

		protected void OnDestroy() {
			water?.RemoveWaterlet(this);
		}
		#endregion
	}
}