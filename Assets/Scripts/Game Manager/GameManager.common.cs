using UnityEngine;

namespace NaniCore.Loopool {
	public partial class GameManager : MonoBehaviour {
		#region Fields
		private LayerMask waterLayer;
		#endregion

		#region Interfaces
		public LayerMask GroundLayerMask => ~(1 << waterLayer);
		#endregion

		#region Life cycle
		protected void StartConstants() {
			waterLayer = LayerMask.NameToLayer("Water");
		}
		#endregion
	}
}