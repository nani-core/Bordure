using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MeshMakerNamespace;


namespace NaniCore {
	public static class MeshUtility {
		#region Mesh data accessing
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
		#endregion

		#region Mesh operations
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

						Mesh result = CSG.Union(ao, bo, false, false);

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

		/// <summary>
		/// Truly merge the meshes together with CSG algorithms.
		/// </summary>
		/// <returns>
		/// A newly created mesh merged from the inputs.
		/// </returns>
		/// <remarks>
		/// Remember to destroy the result mesh to avoid memory leakage.
		/// </remarks>
		public static Mesh MergeMesh(IList<Mesh> meshes) {
			return MergeMeshInternal(meshes.Select(Object.Instantiate).ToList());
		}

		/// <summary>
		/// Make all submeshes into one.
		/// </summary>
		/// <remarks>
		/// Not using CSG algorithms. Result may be ill-formed.
		/// </remarks>
		public static void MergeSubmeshes(this Mesh mesh) {
			var indices = new List<int>();
			foreach(var submeshIndices in mesh.GetSubmeshIndices())
				indices.AddRange(submeshIndices);
			mesh.subMeshCount = 1;
			mesh.SetSubmeshIndices(new List<List<int>> { indices });
		}
		#endregion

		#region GameObject related operations
		/// <remarks>
		/// Not using CSG algorithms. Result may be ill-formed.
		/// </remarks>
		public static Mesh MergeGameObjectIntoMesh(GameObject go) {
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

		public static IEnumerable<(MeshFilter, MeshFilter)> OperateMesh(
			this GameObject target, GameObject shape, CSG.Operation type, float epsilon = 1e-5f,
			Material sectionMaterial = null
			) {
			float oldEpsilon = CSG.EPSILON;
			CSG csg = new CSG();
			CSG.EPSILON = epsilon;

			csg.Brush = shape;
			csg.keepSubmeshes = true;
			csg.OperationType = type;
			csg.hideGameObjects = false;
			foreach(var filter in target.GetComponentsInChildren<MeshFilter>()) {
				if(filter.sharedMesh == null)
					continue;

				csg.Target = filter.gameObject;
				GameObject resultObject;
				try {
					resultObject = csg.PerformCSG();
				}
				catch(System.Exception e) {
					Debug.LogWarning($"Warning: Failed to perform CSG operation on {filter.gameObject}.", filter);
					Debug.LogError(e);
					continue;
				}
				var resultFilter = resultObject.GetComponent<MeshFilter>();
				var resultMesh = resultFilter?.sharedMesh;
				if(resultMesh == null)
					continue;
				resultMesh.name = $"{filter.sharedMesh.name} (operated)";

				// Apply materials for the fresh-cut section faces.
				var resultRenderer = resultObject.GetComponent<Renderer>();
				if(resultRenderer != null) {
					var materialList = resultRenderer.sharedMaterials.ToList();
					materialList[materialList.Count - 1] = sectionMaterial;
					resultRenderer.sharedMaterials = materialList.ToArray();
				}

				yield return (filter, resultFilter);
			}

			CSG.EPSILON = oldEpsilon;
		}
		#endregion
	}
}