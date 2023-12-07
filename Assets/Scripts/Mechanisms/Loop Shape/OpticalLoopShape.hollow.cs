using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore.Loopool {
	public partial class OpticalLoopShape : LoopShape {
		private static readonly List<int> prismIndices = new List<int> {
			/**
			 * 0 --- 1    0 1 2
			 * |\   /|    0 0'1'
			 * | \ / |    1'1 0
			 * |  2  |    1 1'2'
			 * |  |  |    2'2 1
			 * 0'-|--1'   2 2'0
			 *  \ | /     0'0 2
			 *   \|/      
			 *    2'      2'1'0'
			 */
			// Near face
			0, 1, 2,
			// 0 to 1'
			0, 3, 4,
			4, 1, 0,
			// 1 to 2'
			1, 4, 5,
			5, 2, 1,
			// 2 to 0'
			2, 5, 0,
			3, 0, 2,
			// Far face
			2, 1, 0,
		};

		#region Functions
		private static Vector3 Clip(Vector3 point, Vector3 normal) {
			float desiredMag = normal.magnitude;
			float actualMag = point.ProjectOntoAxis(normal).magnitude;
			return point * (desiredMag / actualMag);
		}

		/// <summary>Is facing camera in camera space.</summary>
		private static bool IsFacingCamera(Vector3 a, Vector3 b, Vector3 c) {
			Vector3 normal = Vector3.Cross(b - a, b - c);
			return Vector3.Dot(normal, b) < 0f;
		}

		private static GameObject GenerateHollowShape(GameObject go, Camera camera) {
			if(go == null || camera == null)
				return null;

			Mesh merged = MeshUtility.MergeGameObjectIntoMesh(go);
			merged.ApplyTransform(MathUtility.RelativeTransform(go.transform, camera.transform));

			#region Generate prisms
			List<Mesh> prisms = new List<Mesh>();

			/* Preparation */

			Vector3 nearClip = Vector3.forward * camera.nearClipPlane;
			Vector3 farClip = Vector3.forward * Mathf.Min(camera.farClipPlane, 10);
			// The calculation for these clip planes is technically incorrect.

			var originalVertices = merged.GetVertices();
			var nearVertices = originalVertices.Select(v => {
				v.position = Clip(v.position, nearClip);
				return v;
			}).ToList();
			var farVertices = originalVertices.Select(v => {
				v.position = Clip(v.position, farClip);
				return v;
			}).ToList();

			var indices = new List<int>();
			foreach(var submeshIndices in merged.GetSubmeshIndices())
				indices.AddRange(submeshIndices);

			/* Generation */
			for(int i = 0; i < indices.Count; i += 3) {
				int a = indices[i + 0], b = indices[i + 1], c = indices[i + 2];

				// Back face culling.
				if(!IsFacingCamera(originalVertices[a].position, originalVertices[c].position, originalVertices[b].position))
					continue;

				Mesh prism = new Mesh();

				List<MeshUtility.Vertex> vertices = new List<MeshUtility.Vertex> {
					nearVertices[a], nearVertices[b], nearVertices[c],
					farVertices[a], farVertices[b], farVertices[c],
				};

				prism.SetVertices(vertices);
				prism.SetSubmeshIndices(new List<List<int>> { prismIndices });

				prisms.Add(prism);
			}
			#endregion

			Mesh mergedPrisms = MeshUtility.MergeMesh(prisms);
			mergedPrisms.name = $"{go.name} (hollow shape)";
			// Release intermediate prism meshes.
			foreach(var prism in prisms)
				Destroy(prism);
			prisms.Clear();

			var obj = new GameObject("Hollow Shape");
			obj.transform.SetParent(camera.transform, false);
			obj.transform.SetParent(null, true);

			var filter = obj.AddComponent<MeshFilter>();
			filter.sharedMesh = mergedPrisms;
			var renderer = obj.AddComponent<MeshRenderer>();
			renderer.sharedMaterials = new Material[mergedPrisms.subMeshCount];

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
			GameObject hollowShape = GenerateHollowShape(gastro, Camera.main);

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
		}
		#endregion
	}
}