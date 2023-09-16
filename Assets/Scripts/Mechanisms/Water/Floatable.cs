using UnityEngine;

namespace NaniCore.Loopool {
	[RequireComponent(typeof(Rigidbody))]
	public class Floatable : MonoBehaviour {
		#region Fields
		private new Rigidbody rigidbody;
		#endregion

		#region Functions
		public Rigidbody Rigidbody => rigidbody;
		#endregion

		#region Life cycle
		protected void Start() {
			rigidbody = GetComponent<Rigidbody>();
		}
		#endregion
	}
}