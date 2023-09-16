using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Loopool {
	public abstract class LoopShape : MonoBehaviour {
		#region Static
		protected static HashSet<LoopShape> all;
		public static HashSet<LoopShape> All => all;
		#endregion

		#region Functions
		public abstract bool Satisfied(Transform eye);
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