using UnityEngine;

namespace NaniCore.UnityPlayground {
	public partial class MeshTile : MonoBehaviour {
		#region Serialized fields
		public GameObject tile;
		public Vector3 spacing = Vector3Int.one;
		public Vector3Int count = Vector3Int.one;
		public Vector3 uvw = Vector3.one * .5f;
		#endregion

		#region Functions
		private delegate GameObject Instantiator(GameObject template, Transform under);
		private void Construct(Transform under, Instantiator instantiator) {
			if(tile == null)
				return;
			for(int ix = 0; ix < count.x; ++ix) {
				for(int iy = 0; iy < count.y; ++iy) {
					for(int iz = 0; iz < count.z; ++iz) {
						var instance = instantiator(tile, under).transform;
						instance.localPosition = Vector3.Scale(spacing, new Vector3(ix, iy, iz) - Vector3.Scale(count - Vector3.one, uvw));
					}
				}
			}
		}
		protected void Construct(Transform under) => Construct(under, Instantiate);
		protected void Construct() => Construct(transform);
		#endregion

		#region Life cycle
		protected void Start() {
			Construct();
		}
		#endregion
	}
}