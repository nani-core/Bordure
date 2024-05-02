using UnityEngine;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
		#region Fields
		private LayerMask defaultLayer;
		private LayerMask waterLayer;
		private LayerMask concreteLayer;
		private LayerMask gooseLayer;
		private LayerMask noCollisionLayer;
		#endregion

		#region Interfaces
		public LayerMask WaterLayer => waterLayer;
		public LayerMask GroundLayerMask => ~((1 << waterLayer) | (1 << concreteLayer));
		public LayerMask InteractionLayerMask => (1 << defaultLayer) | (1 << concreteLayer) | (1 << gooseLayer) | (1 << noCollisionLayer);
		#endregion

		#region Life cycle
		protected void InitializeConstants() {
			defaultLayer = LayerMask.NameToLayer("Default");
			waterLayer = LayerMask.NameToLayer("Water");
			concreteLayer = LayerMask.NameToLayer("Concrete");
			gooseLayer = LayerMask.NameToLayer("Goose");
			noCollisionLayer = LayerMask.NameToLayer("NoCollision");
		}
		#endregion
	}
}