using UnityEngine;

namespace NaniCore {
	public partial class MeshTile : MonoBehaviour {
		#region Serialized fields
		public GameObject tile;
		public Vector3 i = Vector3.right, j = Vector3.up, k = Vector3.forward;
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
						var selfOffset = i * ix + j * iy + k * iz;
						instance.localPosition = selfOffset - Vector3.Scale(count - Vector3.one, uvw);
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