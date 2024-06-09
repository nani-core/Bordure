using UnityEngine;

namespace NaniCore.Bordure {
	public partial class GameManager {
		#region Fields
		private int defaultLayer;
		private int waterLayer;
		private int concreteLayer;
		private int noSelfCollisionLayer;
		private int noCollisionSolidLayer;
		private int grabbedLayer;
		#endregion

		#region Interfaces
		public int DefaultLayer => defaultLayer;
		public LayerMask WaterLayer => waterLayer;
		public LayerMask GroundLayerMask => ~((1 << waterLayer) | (1 << concreteLayer));
		public LayerMask InteractionLayerMask => (1 << defaultLayer) | (1 << concreteLayer) | (1 << noSelfCollisionLayer) | (1 << noCollisionSolidLayer);
		public int GrabbedLayer => grabbedLayer;
		#endregion

		#region Life cycle
		protected void InitializeConstants() {
			defaultLayer = LayerMask.NameToLayer("Default");
			waterLayer = LayerMask.NameToLayer("Water");
			concreteLayer = LayerMask.NameToLayer("Concrete");
			noSelfCollisionLayer = LayerMask.NameToLayer("NoSelfCollidion");
			noCollisionSolidLayer = LayerMask.NameToLayer("NoCollisionSolid");
			grabbedLayer = LayerMask.NameToLayer("Grabbed");
		}
		#endregion
	}
}