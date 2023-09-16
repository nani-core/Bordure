using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace NaniCore.Loopool {
	public abstract class LoopShape : MonoBehaviour {
		#region Static
		protected static HashSet<LoopShape> all;
		public static HashSet<LoopShape> All => all;
		#endregion

		#region Serialized fields
		[SerializeField] private UnityEvent onSatisfy;
		[SerializeField] private UnityEvent onUnsatisfy;
		[SerializeField] private UnityEvent onOpen;
		#endregion

		#region Functions
		public abstract bool Satisfied(Transform eye);
		#endregion

		#region Message handlers
		protected void OnLoopShapeSatisfy() {
			onSatisfy?.Invoke();
		}

		protected void OnLoopShapeUnsatisfy() {
			onUnsatisfy?.Invoke();
		}

		protected void OnLoopShapeOpen() {
			onOpen?.Invoke();
			enabled = false;
		}
		#endregion

		#region Life cycle
		protected void Awake() {
			all = new HashSet<LoopShape>();
		}

		protected void Start() {
			all.Add(this);
		}

		protected void OnDestroy() {
			all.Remove(this);
		}
		#endregion
	}
}