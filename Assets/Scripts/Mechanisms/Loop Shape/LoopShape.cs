using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace NaniCore.Stencil {
	public abstract class LoopShape : MonoBehaviour {
		#region Static
		protected static HashSet<LoopShape> all;
		public static HashSet<LoopShape> All {
			get {
				if(all == null)
					all = new HashSet<LoopShape>();
				return all;
			}
		}
		#endregion

		#region Serialized fields
		[Header("Events")]
		[SerializeField] protected UnityEvent onOpen;
		#endregion

		#region Functions
		public abstract bool Validate(Transform eye);
		#endregion

		#region Message handlers
		protected virtual void OnLoopShapeOpen() {
			onOpen?.Invoke();
			enabled = false;
		}
		#endregion

		#region Life cycle
		protected void Start() {
			All.Add(this);
		}

		protected void OnDestroy() {
			All.Remove(this);
		}
		#endregion
	}
}