using UnityEngine;
using System.Collections;

namespace NaniCore.Loopool {
	public partial class OpticalLoopShape : LoopShape {
		private static bool generateFrustumWithSilhouette = false;

		#region Functions
		private static GameObject GenerateHollowShape(GameObject go, Camera camera) {
			if(go == null || camera == null)
				return null;
			
			// Generate the frustum mesh.
			Mesh frustum;
			float
				from = camera.nearClipPlane,
				to = Mathf.Min(20f, camera.farClipPlane);	// This clipping is important to keep CSG behaving normal.
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

		private IEnumerator EjectNeogastroCoroutine(MeshFilter filter, MeshFilter resultFilter) {
			var detachable = neogastro.AddComponent<Detachable>();
			detachable.useDetachingEjection = useDetachingEjection;
			detachable.ejectionVelocity = ejectionVelocity;
			detachable.ejectionOrigin = ejectionOrigin;
			yield return new WaitForEndOfFrame();
			detachable.Detach();
		}

		/// <summary>
		/// Create a hole with gastro's shape on blasto.
		/// </summary>
		/// <remarks>
		/// Needs to be refined to avoid memory leak.
		/// </remarks>
		public void Hollow() {
			GameObject hollowShape = GenerateHollowShape(gastro, GameManager.Instance?.mainCamera);

			float epsilon = 1e-3f;
			// Neogastro.
			// There should only be one `resultFilter` and is assigned to `neoGastro`.
			foreach(var (filter, resultFilter) in blasto.OperateMesh(hollowShape, MeshMakerNamespace.CSG.Operation.Intersection, epsilon, sectionMaterial)) {
				neogastro = resultFilter.gameObject;
				neogastro.name = $"{filter.gameObject.name} (neogastro)";

				// Generate physics
				var resultCollider = neogastro.GetComponent<MeshCollider>();
				if(resultCollider == null)
					resultCollider = neogastro.AddComponent<MeshCollider>();
				resultCollider.sharedMesh = resultFilter.sharedMesh;
				if(resultCollider != null) {
					resultCollider.convex = true;
					var rigidbody = neogastro.GetComponent<Rigidbody>();
					if(useDetachingEjection) {
						if(rigidbody == null)
							rigidbody = neogastro.AddComponent<Rigidbody>();
						StartCoroutine(EjectNeogastroCoroutine(filter, resultFilter));
					}
					if(rigidbody != null) {
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
			foreach(var (filter, resultFilter) in blasto.OperateMesh(hollowShape, MeshMakerNamespace.CSG.Operation.Subtract, epsilon, sectionMaterial)) {
				filter.sharedMesh = resultFilter.sharedMesh;
				filter.GetComponent<MeshCollider>().sharedMesh = resultFilter.sharedMesh;
				filter.GetComponent<Renderer>().sharedMaterials = resultFilter.GetComponent<Renderer>().sharedMaterials;
				Destroy(resultFilter.gameObject);
			}

			Destroy(hollowShape);

			// TODO: Restore original blasto materials.
		}
		#endregion
	}
}