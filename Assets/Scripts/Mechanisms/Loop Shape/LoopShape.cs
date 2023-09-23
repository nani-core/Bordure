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
		[SerializeField] protected GameObject blasto;
		[SerializeField] protected GameObject gastro;

		[Header("Events")]
		[SerializeField] private UnityEvent onValidated;
		[SerializeField] private UnityEvent onInvalidated;
		[SerializeField] private UnityEvent onOpen;
		#endregion

		#region Functions
		public abstract bool Validate(Transform eye);

		public void DestroyGastro() {
			if(gastro == null)
				return;
			gastro.gameObject.SetActive(false);
			gastro = null;
		}
		#endregion

		#region Message handlers
		protected void OnLoopShapeSatisfy() {
			onValidated?.Invoke();
		}

		protected void OnLoopShapeUnsatisfy() {
			onInvalidated?.Invoke();
		}

		protected void OnLoopShapeOpen() {
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