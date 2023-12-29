using UnityEngine;

namespace NaniCore.Loopool {
	public partial class GameManager : MonoBehaviour {
		#region Fields
		private LayerMask waterLayer;
		private LayerMask concreteLayer;
		#endregion

		#region Interfaces
		public LayerMask GroundLayerMask => ~((1 << waterLayer) | (1 << concreteLayer));
		#endregion

		#region Life cycle
		protected void StartConstants() {
			waterLayer = LayerMask.NameToLayer("Water");
			concreteLayer = LayerMask.NameToLayer("Concrete");
		}
		#endregion
	}
}