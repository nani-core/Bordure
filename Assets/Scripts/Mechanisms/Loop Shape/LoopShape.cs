using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace NaniCore.Loopool {
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
		[SerializeField] protected UnityEvent onValidated;
		[SerializeField] protected UnityEvent onInvalidated;
		[SerializeField] protected UnityEvent onOpen;
		#endregion

		#region Functions
		public abstract bool Validate(Transform eye);
		#endregion

		#region Message handlers
		protected virtual void OnLoopShapeSatisfy() {
			onValidated?.Invoke();
		}

		protected virtual void OnLoopShapeUnsatisfy() {
			onInvalidated?.Invoke();
		}

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