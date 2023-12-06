using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore.Loopool {
	public static class LoopoolUtility {
		private static Mesh MergeMesh(GameObject go) {
			if(go == null)
				return null;

			Mesh result = new Mesh();
			result.name = $"{go.name} (merged)";

			var vertices = new List<Vector3>();
			var normals = new List<Vector3>();
			var tangents = new List<Vector4>();
			var uvs = new List<Vector2>();
			var indices = new List<int>();

			foreach(var filter in go.GetComponentsInChildren<MeshFilter>()) {
				var mesh = filter.sharedMesh;
				if(mesh == null)
					continue;

				var indexOffset = vertices.Count;
				var transform = filter.transform.RelativeTransform(go.transform);

				vertices.AddRange(mesh.vertices.Select(transform.MultiplyPoint));
				normals.AddRange(mesh.normals.Select(transform.MultiplyVector));
				tangents.AddRange(mesh.tangents.Select(t => (Vector4)transform.MultiplyVector(t)));
				uvs.AddRange(mesh.uv);
				for(int submeshIndex = 0; submeshIndex < mesh.subMeshCount; ++submeshIndex) {
					indices.AddRange(mesh.GetIndices(submeshIndex).Select(i => i + indexOffset));
				}
			}

			result.SetVertices(vertices);
			result.SetNormals(normals);
			result.SetTangents(tangents);
			result.SetUVs(0, uvs);
			result.SetIndices(indices, MeshTopology.Triangles, 0);

			return result;
		}

		private static IEnumerable<int> MakePrismIndices(int a, int b, int c, int no, int fo) {
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
			yield return no + a;
			yield return no + b;
			yield return no + c;
			// 0 to 1'
			yield return no + a;
			yield return fo + a;
			yield return fo + b;
			yield return fo + b;
			yield return no + b;
			yield return no + a;
			// 1 to 2'
			yield return no + b;
			yield return fo + b;
			yield return fo + c;
			yield return fo + c;
			yield return no + c;
			yield return no + b;
			// 2 to 0'
			yield return no + c;
			yield return fo + c;
			yield return fo + a;
			yield return fo + a;
			yield return no + a;
			yield return no + c;
			// Far face
			yield return fo + c;
			yield return fo + b;
			yield return fo + a;
		}

		private static Vector3 Clip(Vector3 point, Vector3 normal) {
			float desiredMag = normal.magnitude;
			float actualMag = point.ProjectOntoAxis(normal).magnitude;
			return point * (desiredMag / actualMag);
		}

		public static GameObject GenerateHollowShape(GameObject go, Camera camera) {
			if(go == null || camera == null)
				return null;

			#region
			Mesh merged = MergeMesh(go);
			int originalVertexCount = merged.vertexCount;
			Vector3 origin = go.transform.worldToLocalMatrix.MultiplyPoint(camera.transform.position);
			// The calculation for these clip planes is technically incorrect.
			Vector3 cameraForward = go.transform.worldToLocalMatrix.MultiplyVector(camera.transform.forward);
			Vector3 nearClip = cameraForward * camera.nearClipPlane;
			Vector3 farClip = cameraForward * camera.farClipPlane;

			IEnumerable<Vector3> originalPositions = merged.vertices.Select(v => v - origin);
			IEnumerable<Vector3> nearPositions = originalPositions.Select(v => Clip(v, nearClip));
			IEnumerable<Vector3> farPositions = originalPositions.Select(v => Clip(v, farClip));
			var vertices = new List<Vector3>();
			vertices.AddRange(nearPositions);
			vertices.AddRange(farPositions);
			merged.SetVertices(vertices);

			var originalIndices = merged.GetIndices(0);
			var indices = new List<int>();
			for(int i = 0; i < originalIndices.Length; i += 3) {
				var prism = MakePrismIndices(
					originalIndices[i + 0],
					originalIndices[i + 1],
					originalIndices[i + 2],
					0, originalVertexCount
				);
				indices.AddRange(prism);
			}
			merged.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
			#endregion

			var obj = new GameObject("Hollow Shape");
			obj.transform.SetParent(go.transform, false);
			obj.transform.SetParent(null, true);
			obj.transform.position = camera.transform.position;

			var filter = obj.AddComponent<MeshFilter>();
			filter.sharedMesh = merged;
			var renderer = obj.AddComponent<MeshRenderer>();

			return obj;
		}
	}
}