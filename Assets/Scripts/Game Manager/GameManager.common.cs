using UnityEngine;

namespace NaniCore.Loopool {
	public partial class GameManager : MonoBehaviour {
		#region Fields
		private LayerMask defaultLayer;
		private LayerMask waterLayer;
		private LayerMask concreteLayer;
		#endregion

		#region Interfaces
		public LayerMask WaterLayer => waterLayer;
		public LayerMask GroundLayerMask => ~((1 << waterLayer) | (1 << concreteLayer));
		public LayerMask GrabbingLayerMask => (1 << defaultLayer) | (1 << concreteLayer);
		#endregion

		#region Life cycle
		protected void InitializeConstants() {
			defaultLayer = LayerMask.NameToLayer("Default");
			waterLayer = LayerMask.NameToLayer("Water");
			concreteLayer = LayerMask.NameToLayer("Concrete");
		}
		#endregion
	}
}