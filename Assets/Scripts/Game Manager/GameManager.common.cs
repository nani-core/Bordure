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
		public LayerMask GroundLayerMask => ~UnionLayers(waterLayer, concreteLayer);
		public LayerMask InteractionLayerMask => UnionLayers(defaultLayer, concreteLayer, noSelfCollisionLayer, noCollisionSolidLayer, grabbedLayer);
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

		#region Functions
		private static LayerMask UnionLayers(params int[] layers) {
			LayerMask result = 0;
			foreach(var layer in layers) {
				result |= 1 << layer;
			}
			return result;
		}
		#endregion
	}
}