using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MeshMakerNamespace;
using Unity.VisualScripting;
using NaniCore.Loopool;

namespace NaniCore {
	public static class MeshUtility {
		public struct Vertex {
			public Vector3 position;
			public Vector3 normal;
			public Vector4 tangent;
			public Vector2 uv;
			// uv1...uv8

			public Vertex Apply(Matrix4x4 transform) {
				Vertex result = this;
				result.position = transform.MultiplyPoint(position);
				result.normal = transform.MultiplyVector(normal);
				result.tangent = transform.MultiplyVector(tangent);
				return result;
			}
		}

		/// <summary>
		/// Returns the vertices of the mesh in a grouped format.
		/// </summary>
		public static List<Vertex> GetVertices(this Mesh mesh) {
			var positions = mesh.vertices;
			var normals = mesh.normals;
			var tangents = mesh.tangents;
			var uvs = mesh.uv;

			var result = new List<Vertex>();
			for(int i = 0; i < mesh.vertexCount; ++i) {
				Vertex vertex;
				vertex.position = positions[i];
				vertex.normal = normals[i];
				vertex.tangent = tangents[i];
				vertex.uv = uvs[i];
				result.Add(vertex);
			}
			return result;
		}

		public static void SetVertices(this Mesh mesh, IList<Vertex> vertices) {
			int count = vertices.Count();

			var positions = new Vector3[count];
			var normals = new Vector3[count];
			var tangents = new Vector4[count];
			var uvs = new Vector2[count];

			for(int i = 0; i < vertices.Count; ++i) {
				var vertex = vertices[i];
				positions[i] = vertex.position;
				normals[i] = vertex.normal;
				tangents[i] = vertex.tangent;
				uvs[i] = vertex.uv;
			}

			mesh.SetVertices(positions);
			mesh.normals = normals;
			mesh.tangents = tangents;
			mesh.uv = uvs;
		}

		public static List<List<int>> GetSubmeshIndices(this Mesh mesh) {
			int submeshCount = mesh.subMeshCount;
			var result = new List<List<int>>();
			for(int submesh = 0; submesh < submeshCount; ++submesh) {
				var submeshIndices = new List<int>(mesh.GetIndices(submesh));
				result.Add(submeshIndices);
			}
			return result;
		}

		public static void SetSubmeshIndices(this Mesh mesh, List<List<int>> all, MeshTopology topology = MeshTopology.Triangles) {
			mesh.subMeshCount = all.Count;
			for(int submesh = 0; submesh < all.Count; ++submesh) {
				var submeshIndices = new List<int>();
				if(submesh < mesh.subMeshCount)
					submeshIndices.AddRange(mesh.GetIndices(submesh));
				submeshIndices.AddRange(all[submesh]);
				mesh.SetIndices(submeshIndices, topology, submesh);
			}
		}

		public static void ApplyTransform(this Mesh mesh, Matrix4x4 transform) {
			if(mesh == null)
				return;

			var vertices = mesh.GetVertices();
			for(int i = 0; i < vertices.Count; ++i) {
				vertices[i] = vertices[i].Apply(transform);
			}
			mesh.SetVertices(vertices);
		}

		public static void Append(this Mesh mesh, IEnumerable<Mesh> appends) {
			if(mesh == null)
				return;

			var vertices = mesh.GetVertices();
			var submeshIndices = mesh.GetSubmeshIndices();

			foreach(var append in appends) {
				if(append == null)
					continue;

				var offset = vertices.Count;
				vertices.AddRange(append.GetVertices());
				submeshIndices.AddRange(append.GetSubmeshIndices().Select(list => list.Select(i => i + offset).ToList()));
			}

			mesh.SetVertices(vertices);
			mesh.SetSubmeshIndices(submeshIndices);
		}

		private static Mesh MergeGameObjectIntoMesh(GameObject go) {
			if(go == null)
				return null;

			var appends = go.GetComponentsInChildren<MeshFilter>()
				.Select(filter => {
					Mesh mesh = filter.sharedMesh;
					if(mesh == null)
						return null;
					mesh = Object.Instantiate(mesh);
					var transform = filter.transform.RelativeTransform(go.transform);
					mesh.ApplyTransform(transform);
					return mesh;
				});

			Mesh result = new Mesh();
			result.name = $"{go.name} (merged)";
			result.Append(appends);

			foreach(Mesh mesh in appends) {
				Object.Destroy(mesh);
			}
			appends = null;

			return result;
		}

		/// <param name="meshes">A list of temporary mesh. All meshes inside will be destroyed after call.</param>
		private static Mesh MergeMeshInternal(IList<Mesh> meshes) {
			switch(meshes.Count) {
				case 0:
					return null;
				case 1:
					return meshes[0];
				case 2: {
						Mesh a = meshes[0], b = meshes[1];

						GameObject ao = new GameObject(), bo = new GameObject();
						ao.AddComponent<MeshFilter>().sharedMesh = a;
						bo.AddComponent<MeshFilter>().sharedMesh = b;

						Mesh result = CSG.Union(ao, bo, true, false);

						Object.Destroy(ao);
						Object.Destroy(bo);

						Object.Destroy(a);
						Object.Destroy(b);

						return result;
					}
				default: {
						int prevCount = meshes.Count / 2;

						Mesh a = MergeMeshInternal(meshes.Take(prevCount).ToList());
						Mesh b = MergeMeshInternal(meshes.Take(prevCount).ToList());

						Mesh result = MergeMeshInternal(new Mesh[] { a, b });

						Object.Destroy(a);
						Object.Destroy(b);

						return result;
					}
			}
		}

		/// <returns>A newly created mesh merged from the inputs.</returns>
		/// <remarks>Remember to destroy the result mesh to avoid memory leakage.</remarks>
		public static Mesh MergeMesh(IList<Mesh> meshes) {
			return MergeMeshInternal(meshes.Select(Object.Instantiate).ToList());
		}

		public static void MergeSubmeshes(this Mesh mesh) {
			var indices = new List<int>();
			foreach(var submeshIndices in mesh.GetSubmeshIndices())
				indices.AddRange(submeshIndices);
			mesh.subMeshCount = 1;
			mesh.SetSubmeshIndices(new List<List<int>> { indices });
		}

		private static IEnumerable<int> MakePrismIndices(int a, int b, int c, int nearOffset, int farOffset) {
			/**
			 * 0 --- 1    0 1 2
			 * |\   /|    0 0'1'
			 * | \ / |    1'1 0
			 * |  2  |    1 1'2'
			 * |  |  |    2'2 1'
			 * 0'-|--1'   2 2'0
			 *  \ | /     0'0 2
			 *   \|/      
			 *    2'      2'1'0'
			 */
			// Near face
			yield return nearOffset + a;
			yield return nearOffset + b;
			yield return nearOffset + c;
			// 0 to 1'
			yield return nearOffset + a;
			yield return farOffset + a;
			yield return farOffset + b;
			yield return farOffset + b;
			yield return nearOffset + b;
			yield return nearOffset + a;
			// 1 to 2'
			yield return nearOffset + b;
			yield return farOffset + b;
			yield return farOffset + c;
			yield return farOffset + c;
			yield return nearOffset + c;
			yield return nearOffset + b;
			// 2 to 0'
			yield return nearOffset + c;
			yield return farOffset + c;
			yield return farOffset + a;
			yield return farOffset + a;
			yield return nearOffset + a;
			yield return nearOffset + c;
			// Far face
			yield return farOffset + c;
			yield return farOffset + b;
			yield return farOffset + a;
		}

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

		public static GameObject GenerateHollowShape(GameObject go, Camera camera) {
			if(go == null || camera == null)
				return null;

			Mesh merged = MergeGameObjectIntoMesh(go);
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
			var prismIndices = MakePrismIndices(0, 1, 2, 0, 3).ToList();
			for(int i = 0; i < indices.Count; i += 3) {
				int a = indices[i + 0], b = indices[i + 1], c = indices[i + 2];

				// Back face culling.
				if(!IsFacingCamera(originalVertices[a].position, originalVertices[c].position, originalVertices[b].position))
					continue;

				Mesh prism = new Mesh();

				List<Vertex> vertices = new List<Vertex> {
					nearVertices[a], nearVertices[b], nearVertices[c],
					farVertices[a], farVertices[b], farVertices[c],
				};

				prism.SetVertices(vertices);
				prism.SetSubmeshIndices(new List<List<int>> { prismIndices });

				prisms.Add(prism);
			}
			#endregion

			Mesh mergedPrisms;
			if(false) {
				// This causes stack overflow.
				mergedPrisms = MergeMesh(prisms);
			}
			else {
				// Debug option, prisms not properly merged.
				mergedPrisms = new Mesh();
				// Use this line to use all prisms.
				//mergedPrisms.Append(prisms);
				mergedPrisms.Append(prisms.Take(1));
				mergedPrisms.MergeSubmeshes();
			}
			mergedPrisms.name = $"{go.name} (hollow shape)";
			// Release intermediate prism meshes.
			foreach(var prism in prisms)
				Object.Destroy(prism);
			prisms.Clear();

			var obj = new GameObject("Hollow Shape");
			obj.transform.SetParent(camera.transform, false);
			obj.transform.SetParent(null, true);

			var filter = obj.AddComponent<MeshFilter>();
			filter.sharedMesh = mergedPrisms;
			var renderer = obj.AddComponent<MeshRenderer>();
			var materials = new Material[mergedPrisms.subMeshCount];
			for(int i = 0; i < mergedPrisms.subMeshCount; ++i)
				materials[i] = GameManager.Instance.hollowShapeMaterial;
			renderer.sharedMaterials = materials;

			return obj;
		}
	}
}