using UnityEngine;
using MeshMakerNamespace;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore.Bordure {
	public class HollowingManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField] protected Material sectionMaterial;
		#endregion

		#region Constants
		private const bool generateFrustumWithSilhouette = false;
		#endregion

		#region Functions
		private static GameObject GenerateHollowShape(GameObject go, Camera camera) {
			if(go == null || camera == null)
				return null;

			// Generate the frustum mesh.
			Mesh frustum;
			float
				from = camera.nearClipPlane,
				to = Mathf.Min(20f, camera.farClipPlane);   // This clipping is important to keep CSG behaving normal.
#pragma warning disable CS0162
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
#pragma warning restore CS0162

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
			// Create the renderer-material pair for the blasto.
			Dictionary<Renderer, IEnumerable<Material>> matPairs = new(
				gameObject.GetComponentsInChildren<Renderer>(true)
					.Select(renderer => new KeyValuePair<Renderer, IEnumerable<Material>>(renderer, renderer.sharedMaterials))
			);

			StampHandler.Stamp(gameObject, GameManager.Instance?.MainCamera);

			GameObject hollowShape = GenerateHollowShape(shape, GameManager.Instance?.MainCamera);

			float epsilon = 1e-3f;
			// Create the intersecting gastro object.
			// There should only be one `resultFilter` and is assigned to `neoGastro`.
			foreach(var (filter, resultFilter) in gameObject.OperateMesh(hollowShape, CSG.Operation.Intersection, epsilon, sectionMaterial)) {
				var hollowedObject = resultFilter.gameObject;
				hollowedObject.name = $"{filter.gameObject.name} (hollowed)";

				// Generate physics
				var resultCollider = hollowedObject.EnsureComponent<MeshCollider>();
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

			// Create the subtracting blasto frame object.
			foreach(var (filter, resultFilter) in gameObject.OperateMesh(hollowShape, CSG.Operation.Subtract, epsilon, sectionMaterial)) {
				filter.sharedMesh = resultFilter.sharedMesh;
				filter.EnsureComponent<MeshCollider>().sharedMesh = resultFilter.sharedMesh;
				List<Material> newMatList = new();
				var renderer = filter.EnsureComponent<MeshRenderer>();
				if(matPairs.ContainsKey(renderer) && false) {
					// Use original mats.
					// Can't do. UV has been altered by CSG.
					newMatList.AddRange(matPairs[renderer]);
					newMatList.Add(sectionMaterial);
				}
				else {
					// The last material should be the one for the sections.
					newMatList.AddRange(resultFilter.GetComponent<Renderer>().sharedMaterials);
				}
				renderer.sharedMaterials = newMatList.ToArray();

				Destroy(resultFilter.gameObject);
			}

			Destroy(hollowShape);
		}
	}
	#endregion
}