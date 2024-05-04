using UnityEngine;

namespace NaniCore.Bordure {
	public partial class GameManager {
		#region Fields
		private LayerMask defaultLayer;
		private LayerMask waterLayer;
		private LayerMask concreteLayer;
		private LayerMask noSelfCollisionLayer;
		private LayerMask noCollisionSolidLayer;
		#endregion

		#region Interfaces
		public LayerMask WaterLayer => waterLayer;
		public LayerMask GroundLayerMask => ~((1 << waterLayer) | (1 << concreteLayer));
		public LayerMask InteractionLayerMask => (1 << defaultLayer) | (1 << concreteLayer) | (1 << noSelfCollisionLayer) | (1 << noCollisionSolidLayer);
		#endregion

		#region Life cycle
		protected void InitializeConstants() {
			defaultLayer = LayerMask.NameToLayer("Default");
			waterLayer = LayerMask.NameToLayer("Water");
			concreteLayer = LayerMask.NameToLayer("Concrete");
			noSelfCollisionLayer = LayerMask.NameToLayer("NoSelfCollidion");
			noCollisionSolidLayer = LayerMask.NameToLayer("NoCollisionSolid");
		}
		#endregion
	}
}