using UnityEngine;
using MeshMakerNamespace;

namespace NaniCore.Stencil {
	public class HollowingManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField] protected Material sectionMaterial;
		#endregion
		private static bool generateFrustumWithSilhouette = false;

		#region Functions
		private static GameObject GenerateHollowShape(GameObject go, Camera camera) {
			if(go == null || camera == null)
				return null;

			// Generate the frustum mesh.
			Mesh frustum;
			float
				from = camera.nearClipPlane,
				to = Mathf.Min(20f, camera.farClipPlane);   // This clipping is important to keep CSG behaving normal.
			if(generateFrustumWithSilhouette) {
				var mask = RenderUtility.CreateScreenSizedRT();
				float referenceSize = mask.width;
				mask.RenderMask(go, camera);
				frustum = mask.SilhouetteToFrustum(from, to, referenceSize);
				mask.Destroy();
			}
			else {
				Mesh whole = go.MergeGameObjectIntoMesh();
				whole.ApplyTransform(camera.transform.worldToLocalMatrix * go.transform.localToWorldMatrix);
				frustum = whole.BaseMeshToFrustum(from, to);
				Destroy(whole);
			}

			var obj = new GameObject("Hollow Shape");
			obj.transform.SetParent(camera.transform, false);
			obj.transform.SetParent(null, true);

			var filter = obj.AddComponent<MeshFilter>();
			filter.sharedMesh = frustum;
			var renderer = obj.AddComponent<MeshRenderer>();
			renderer.sharedMaterials = new Material[frustum.subMeshCount];

			return obj;
		}

		/// <remarks>
		/// Needs to be refined to avoid memory leak.
		/// </remarks>
		public void Hollow(GameObject shape) {
			StampHandler.Stamp(gameObject, GameManager.Instance?.MainCamera);

			GameObject hollowShape = GenerateHollowShape(shape, GameManager.Instance?.MainCamera);

			float epsilon = 1e-3f;
			// There should only be one `resultFilter` and is assigned to `neoGastro`.
			foreach(var (filter, resultFilter) in gameObject.OperateMesh(hollowShape, CSG.Operation.Intersection, epsilon, sectionMaterial)) {
				var hollowedObject = resultFilter.gameObject;
				hollowedObject.name = $"{filter.gameObject.name} (hollowed)";

				// Generate physics
				if(!hollowedObject.TryGetComponent<MeshCollider>(out var resultCollider))
					resultCollider = hollowedObject.AddComponent<MeshCollider>();
				resultCollider.sharedMesh = resultFilter.sharedMesh;
				if(resultCollider != null) {
					resultCollider.convex = true;
					if(hollowedObject.TryGetComponent<Rigidbody>(out var rigidbody)) {
						rigidbody.constraints = RigidbodyConstraints.None;
						try {
							rigidbody.isKinematic = false;
						}
						catch(System.Exception e) {
							Debug.Log("Warning: Cannot set hollowed-out rigidbody to be non-kinematic. Probably due to non-convexity.", rigidbody);
							Debug.LogException(e);
						}
					}
				}
			}
			foreach(var (filter, resultFilter) in gameObject.OperateMesh(hollowShape, CSG.Operation.Subtract, epsilon, sectionMaterial)) {
				filter.sharedMesh = resultFilter.sharedMesh;
				filter.GetComponent<MeshCollider>().sharedMesh = resultFilter.sharedMesh;
				filter.GetComponent<Renderer>().sharedMaterials = resultFilter.GetComponent<Renderer>().sharedMaterials;
				Destroy(resultFilter.gameObject);
			}

			Destroy(hollowShape);

			// TODO: Restore original gameObject materials.
		}
		#endregion
	}
}