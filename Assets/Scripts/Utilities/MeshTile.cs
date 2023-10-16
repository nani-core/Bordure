using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore {
	public partial class MeshTile : MonoBehaviour {
		#region Serialized fields
		public GameObject tile;
		public Vector3 i = Vector3.right, j = Vector3.up, k = Vector3.forward;
		public Vector3Int count = Vector3Int.one;
		public Vector3 uvw = Vector3.one * .5f;
		public List<GameObject> hollowObjects = new List<GameObject>();
		#endregion

		#region Functions
		private delegate GameObject Instantiator(GameObject template, Transform under);
		private void Construct(Transform under, Instantiator instantiator) {
			if(tile == null)
				return;
			for(int ix = 0; ix < count.x; ++ix) {
				for(int iy = 0; iy < count.y; ++iy) {
					for(int iz = 0; iz < count.z; ++iz) {
						var selfOffset = i * ix + j * iy + k * iz;
						Vector3 localPosition = selfOffset - Vector3.Scale(count - Vector3.one, uvw);
						Vector3 worldPosition = under.localToWorldMatrix.MultiplyPoint(localPosition);
						if(hollowObjects.Any(o => CheckIfPointIsIn(worldPosition, o)))
							continue;
						var instance = instantiator(tile, under).transform;
						instance.localPosition = localPosition;
					}
				}
			}
		}
		protected void Construct(Transform under) => Construct(under, Instantiate);
		protected void Construct() => Construct(transform);

		private static bool CheckIfPointIsIn(Vector3 worldPosition, GameObject gameObject) {
			if(gameObject == null)
				return false;
			foreach(var collider in gameObject.GetComponentsInChildren<Collider>(true)) {
				if(CheckIfPointIsIn(worldPosition, collider))
					return true;
			}
			return false;
		}
		private static bool CheckIfPointIsIn(Vector3 worldPosition, Collider collider) {
			if(collider == null)
				return false;
			switch(collider) {
				default:
					Debug.LogWarning($"Mesh tile hollow object does not support collider of type {collider.GetType().Name}!");
					return false;
				case BoxCollider box: {
						Vector3 local = box.transform.worldToLocalMatrix.MultiplyPoint(worldPosition);
						local -= box.center;
						local *= 2f;
						return
							(Mathf.Abs(local.x) <= box.size.x) &&
							(Mathf.Abs(local.y) <= box.size.y) &&
							(Mathf.Abs(local.z) <= box.size.z);
					}
				case SphereCollider sphere: {
						float radius = sphere.radius * sphere.transform.lossyScale.magnitude / Mathf.Sqrt(3);
						return Vector3.Distance(worldPosition, sphere.transform.position) <= radius;
					}
			}
		}
		#endregion

		#region Life cycle
		protected void Start() {
			Construct();
			Destroy(this);
		}
		#endregion
	}
}