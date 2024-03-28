using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Nianyi {
	public partial class MeshEx {
		#region Default constants
		private const int defaultCutIteration = 6;
		private static readonly bool useCutDebug = true;
		private const int debugMaxCutIteration = 1;
		private const float debugCutChance = .2f;
		#endregion

		#region Subclasses
		public struct Descriptor {
			public bool useNormals;
			public bool useTangents;
			public bool useColors;
			public uint uvMask;
		}

		public class Vertex {
			public Vector3 position;
			public Vector3 normal = default;
			public Vector4 tangent = default;
			public Vector2[] uvs = new Vector2[8];
			public Color color = Color.white;

			#region Constructors
			public Vertex() { }
			public Vertex(Vertex vertex) : this() {
				Assign(vertex);
			}
			#endregion

			#region Interfaces
			/// <summary>
			/// Apply a matrix transformation on this vertex in place.
			/// </summary>
			public void Transform(in Matrix4x4 transformation) {
				position = TransformPosition(position, transformation);
				normal = TransformNormal(normal, transformation);
				tangent = TransformTangent(tangent, transformation);
			}

			/// <summary>Copy data from another vertex instance.</summary>
			public void Assign(in Vertex vertex) {
				position = vertex.position;
				normal = vertex.normal;
				tangent = vertex.tangent;
				for(int i = 0; i < 8; ++i)
					uvs[i] = vertex.uvs[i];
				color = vertex.color;
			}
			#endregion

			#region Transformations
			public static Vertex Transform(in Vertex vertex, in Matrix4x4 transformation) {
				Vertex copy = new(vertex);
				copy.Transform(transformation);
				return copy;
			}

			public static Vector3 TransformPosition(in Vector3 position, in Matrix4x4 transformation) {
				return transformation.MultiplyPoint(position);
			}

			public static Vector3 TransformNormal(in Vector3 normal, in Matrix4x4 transformation) {
				return transformation.inverse.transpose.MultiplyVector(normal).normalized;
			}

			public static Vector3 TransformTangent(in Vector4 tangent, in Matrix4x4 transformation) {
				// TODO: This is likely incorrect.
				return transformation.MultiplyVector(tangent).normalized;
			}

			public static Vertex Lerp(float t, Vertex a, Vertex b) {
				return new() {
					position = Vector3.Lerp(a.position, b.position, t),
					normal = Vector3.Slerp(a.normal, b.normal, t),
					tangent = Vector3.Slerp(a.tangent, b.tangent, t),
					uvs = a.uvs.Select((auv, i) => Vector2.Lerp(auv, b.uvs[i], t)).ToArray(),
					color = Color.Lerp(a.color, b.color, t),
				};
			}
			#endregion
		}

		public class Triangle {
			private readonly Vertex a, b, c;

			#region Constructors
			public Triangle(in Vertex a, in Vertex b, in Vertex c) {
				this.a = a;
				this.b = b;
				this.c = c;
			}
			public Triangle(params Vertex[] vertices) : this(vertices[0], vertices[1], vertices[2]) { }
			public Triangle(in Triangle triangle) : this(triangle.Vertices.Select(v => new Vertex(v)).ToArray()) { }
			#endregion

			#region Interfaces
			public Vertex A => a;
			public Vertex B => b;
			public Vertex C => c;

			public Vertex[] Vertices => new Vertex[] { A, B, C };
			public Vertex NextVertexOf(in Vertex vertex) {
				if(vertex == A)
					return B;
				if(vertex == B)
					return C;
				if(vertex == C)
					return A;
				return null;
			}
			public Vertex PreviousVertexOf(in Vertex vertex) {
				if(vertex == A)
					return C;
				if(vertex == B)
					return A;
				if(vertex == C)
					return B;
				return null;
			}
			public (Vertex, Vertex, Vertex) CycleVerticesFrom(in Vertex start) {
				if(start == A)
					return (A, B, C);
				if(start == b)
					return (b, C, A);
				if(start == C)
					return (C, A, B);
				return default;
			}

			public Vector3 CheapNormal => Vector3.Cross(B.position - A.position, C.position - B.position);
			public Vector3 Normal => Vector3.Cross(
				(B.position - A.position).normalized,
				C.position - B.position
			).normalized;

			public Matrix4x4 TriangleToMeshMatrix {
				get {
					Vector3
						i = B.position - A.position,
						j = C.position - A.position,
						k = Vector3.Cross(i.normalized, j).normalized;
					return Matrix4x4.Translate(A.position) * new Matrix4x4(i, j, k, new(0, 0, 0, 1));
				}
			}
			public Matrix4x4 MeshToTriangleMatrix => TriangleToMeshMatrix.inverse;

			/// <summary>
			/// Transform the vertices of a triangle in place.
			/// </summary>
			public void Transform(in Matrix4x4 transformation) {
				foreach(var vertex in Vertices)
					vertex.Transform(transformation);
			}

			/// <summary>Cut this triangle by another cutting triangle.</summary>
			/// <param name="cut">The cutting triangle.</param>
			/// <param name="results">The cut slices of the original triangle, if they intersect.</param>
			/// <returns>Whether this triangle intersects with the cutting triangle.</returns>
			public bool Cut(in Triangle cut, out Triangle[] results) {
				if(useCutDebug)
					return CutDebug(cut, out results);
				return CutPrecise(cut, out results);
			}

			private bool CutDebug(in Triangle cut, out Triangle[] results) {
				if(Random.value < debugCutChance) {
					results = null;
					return false;
				}

				var (a, b, c) = CycleVerticesFrom(Vertices[0]);
				var ab = Vertex.Lerp(.5f, a, b);
				var bc = Vertex.Lerp(.5f, b, c);
				var ca = Vertex.Lerp(.5f, c, a);

				results = new Triangle[] {
					new(a, ab, ca),
					new(b, bc, ab),
					new(c, ca, bc),
					new(ab, bc, ca),
				};
				return true;
			}

			/// <summary>Cut this triangle by another cutting triangle.</summary>
			/// <param name="cut">The cutting triangle.</param>
			/// <param name="results">The cut slices of the original triangle, if they intersect.</param>
			/// <returns>Whether this triangle intersects with the cutting triangle.</returns>
			private bool CutPrecise(in Triangle cut, out Triangle[] results) {
				results = new Triangle[0];

				Triangle cutCopy = new(cut);
				cutCopy.Transform(MeshToTriangleMatrix);

				// (d, e, f) are the vertices of the cutting triangles in the cut space.
				var d = DetermineBadBoy(cutCopy);
				if(d == null)
					return false;
				var (_, e, f) = cutCopy.CycleVerticesFrom(d);

				Vector3 u, v;
				float ut, vt;
				if(
					!RayCastOnXYPlane(d.position, e.position - d.position, out u, out ut) ||
					!InRangeInclusive(ut, 0, 1) ||
					!RayCastOnXYPlane(d.position, f.position - d.position, out v, out vt) ||
					!InRangeInclusive(vt, 0, 1)
				)
					return false;

				// We now have (u, v) as the two intersecting points of the cutting triangle on the target triangle.
				// They should have a Z coordinate of 0.

				// (g, h, i) are the original vertices (a, b, c) in the cut space.
				var (g, h, i) = (new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1));
				float ght, hit, igt;
				Ray2D uv = new(u, v - u);

				// Now we can decide the top and two bottom vertices and their cutting portion.
				// (left, top, right) are the vertices in the mesh's local space.
				Vertex left = null, top = null, right = null;
				float leftt = 0, rightt = 0;

				TwoRayCast2D(g, h, uv, out _, out ght);
				TwoRayCast2D(h, i, uv, out _, out hit);
				TwoRayCast2D(i, g, uv, out _, out igt);

				int failedCastCount = 0;
				if(!InRangeInclusive(ght, 0, 1)) {
					(left, top, right) = (B, C, A);
					(leftt, rightt) = (hit, igt);
					++failedCastCount;
				}
				if(!InRangeInclusive(hit, 0, 1)) {
					(left, top, right) = (C, A, B);
					(leftt, rightt) = (igt, ght);
					++failedCastCount;
				}
				if(!InRangeInclusive(igt, 0, 1)) {
					(left, top, right) = (A, B, C);
					(leftt, rightt) = (ght, hit);
					++failedCastCount;
				}
				// How could it be zero??
				if(failedCastCount != 1)
					return false;

				// Last, generate the cut result.
				Vertex
					leftCut = Vertex.Lerp(leftt, top, left),
					rightCut = Vertex.Lerp(rightt, top, right);

				results = new Triangle[] {
					new(top, left, right),
					new(left, right, leftCut),
					new(right, rightCut, left),
				};

				return true;
			}
			#endregion

			#region Functions
			private static Vertex DetermineBadBoy(in Triangle triangle) {
				var vertices = triangle.Vertices;
				int[] d = vertices.Select(v => (int)Mathf.Sign(v.position.z)).ToArray();

				int zeroCount = d.Count(d => d == 0), sum = 0;
				Vertex firstNonZero = null;
				for(int i = 0; i < 3; ++i) {
					sum += d[i];
					if(d[i] != 0)
						firstNonZero = vertices[i];
				}

				if(zeroCount != 0) {
					if(zeroCount == 1 || zeroCount == 2)
						return firstNonZero;
					// 3 zeros, Basically impossible.
					return null;
				}

				// Normal cases.
				for(int i = 0; i < 3; ++i) {
					if(d[i] * sum < 0)
						return vertices[i];
				}
				return null;
			}
			#endregion
		}

		public class Submesh {
			private readonly HashSet<Vertex> vertices = new();
			private readonly HashSet<Triangle> triangles = new();

			public HashSet<Vertex> Vertices => vertices;
			public HashSet<Triangle> Triangles => triangles;

			public Triangle CreateTriangle(params Vertex[] vertices) {
				foreach(var vertex in vertices) {
					if(!this.vertices.Contains(vertex))
						this.vertices.Add(vertex);
				}
				Triangle triangle = new(vertices);
				triangles.Add(triangle);
				return triangle;
			}
			public Triangle CreateTriangle(in Vertex a, in Vertex b, in Vertex c)
				=> CreateTriangle(new Vertex[] { a, b, c });
			public bool AddTriangle(in Triangle triangle) {
				if(triangles.Contains(triangle))
					return false;
				foreach(var vertex in triangle.Vertices)
					vertices.Add(vertex);
				triangles.Add(triangle);
				return true;
			}
			/// <remarks>You might want to call `TrimVertices()` afterwards.</remarks>
			/// <returns>Whether the triangle is successfully removed.</returns>
			public bool RemoveTriangle(in Triangle triangle) {
				return triangles.Remove(triangle);
			}

			/// <summary>Trim all vertices that are not referenced by any triangles.</summary>
			/// <remarks>
			/// This function should not be called frequently.
			/// You can call it after some heavy operations to clean up.
			/// </remarks>
			public void TrimVertices() {
				HashSet<Vertex> referencedVertices = new();
				foreach(var triangle in triangles) {
					foreach(var vertex in triangle.Vertices)
						referencedVertices.Add(vertex);
				}
				vertices.RemoveWhere(v => !referencedVertices.Contains(v));
			}
		}
		#endregion

		#region Fields
		private readonly List<Submesh> submeshes = new();
		public Descriptor descriptor = new() {
			useNormals = true,
			useTangents = false,
			useColors = false,
			uvMask = 1,
		};
		#endregion

		#region Constructors and conversions
		public MeshEx() { }

		public MeshEx(in Mesh mesh) {
			if(!mesh.isReadable)
				throw new UnityException($"{mesh} must be readable in order to be analyzed.");

			// Load vertices.
			Vertex[] vertices = new Vertex[mesh.vertexCount];
			{
				Vector3[] positions = mesh.vertices;
				Vector3[] normals = mesh.normals;
				descriptor.useNormals = normals.Length > 0;
				Vector4[] tangents = mesh.tangents;
				descriptor.useTangents = tangents.Length > 0;
				Vector2[][] uvs = new Vector2[][] {
					mesh.uv, mesh.uv2, mesh.uv3, mesh.uv4, mesh.uv5, mesh.uv6, mesh.uv7, mesh.uv8
				};
				for(int channel = 0; channel < uvs.Length; ++channel) {
					if(uvs[channel] != null && uvs[channel].Length > 0)
						descriptor.uvMask |= (1u << channel);
				}
				Color[] colors = mesh.colors;
				descriptor.useColors = colors.Length > 0;
				for(int i = 0; i < mesh.vertexCount; ++i) {
					Vertex v = new();

					v.position = positions[i];
					if(descriptor.useNormals) v.normal = normals[i];
					if(descriptor.useTangents) v.tangent = tangents[i];
					for(int channel = 0; channel < uvs.Length; ++channel) {
						if(IsUsingUvChannel(channel))
							v.uvs[channel] = uvs[channel][i];
					}
					if(descriptor.useColors) v.color = colors[i];

					vertices[i] = v;
				}
			}

			// Load submeshes.
			for(int submeshIndex = 0; submeshIndex < mesh.subMeshCount; ++submeshIndex) {
				int[] submeshVertexIndices = mesh.GetIndices(submeshIndex);
				Submesh submesh = new();
				for(int i = 0; i < submeshVertexIndices.Length; i += 3) {
					submesh.CreateTriangle(new Vertex[] {
						vertices[submeshVertexIndices[i + 0]],
						vertices[submeshVertexIndices[i + 1]],
						vertices[submeshVertexIndices[i + 2]],
					});
				}
				submeshes.Add(submesh);
			}
		}

		public MeshEx(in MeshEx meshEx) {
			descriptor = meshEx.descriptor;
			foreach(var submesh in meshEx.Submeshes) {
				var newSubmesh = CreateSubmesh();
				foreach(var triangle in submesh.Triangles) {
					var verticesCopy = triangle.Vertices.Select(v => new Vertex(v)).ToArray();
					newSubmesh.CreateTriangle(verticesCopy);
				}
			}
		}

		public Mesh ToMesh() {
			List<Vector3> positions = new();
			List<Vector3> normals = new();
			List<Vector4> tangents = new();
			List<Vector2>[] uvs = new List<Vector2>[8];
			for(int channel = 0; channel < 8; ++channel)
				uvs[channel] = new();
			List<Color> colors = new();

			List<List<int>> submeshIndexList = new();

			int vertexIndex = 0;
			foreach(var submesh in Submeshes) {
				Dictionary<Vertex, int> vertexIndexLookUp = new();
				// Add vertex data.
				foreach(var vertex in submesh.Vertices) {
					vertexIndexLookUp.Add(vertex, vertexIndex++);

					positions.Add(vertex.position);
					if(descriptor.useNormals) normals.Add(vertex.normal);
					if(descriptor.useTangents) tangents.Add(vertex.tangent);
					for(int channel = 0; channel < 8; ++channel) {
						if(IsUsingUvChannel(channel))
							uvs[channel].Add(vertex.uvs[channel]);
					}
					if(descriptor.useColors) colors.Add(vertex.color);
				}
				// Add triangle indices.
				List<int> submeshIndices = new();
				foreach(var triangle in submesh.Triangles) {
					foreach(var vertex in triangle.Vertices)
						submeshIndices.Add(vertexIndexLookUp[vertex]);
				}
				submeshIndexList.Add(submeshIndices);
			}

			// Create the mesh.
			Mesh mesh = new();
			mesh.SetVertices(positions);
			if(descriptor.useNormals) mesh.SetNormals(normals);
			if(descriptor.useTangents) mesh.SetTangents(tangents);
			for(int channel = 0; channel < 8; ++channel) {
				if(IsUsingUvChannel(channel))
					mesh.SetUVs(channel, uvs[channel]);
			}
			if(descriptor.useColors) mesh.SetColors(colors);

			mesh.subMeshCount = submeshIndexList.Count;
			for(int submeshIndex = 0; submeshIndex < submeshIndexList.Count; ++submeshIndex) {
				mesh.SetIndices(submeshIndexList[submeshIndex], MeshTopology.Triangles, submeshIndex);
			}

			return mesh;
		}
		#endregion

		#region Interfaces
		public IList<Submesh> Submeshes => submeshes;
		public IEnumerable<Vertex> Vertices {
			get {
				foreach(var submesh in Submeshes) {
					foreach(var vertex in submesh.Vertices) {
						yield return vertex;
					}
				}
			}
		}
		public IEnumerable<Triangle> Triangles {
			get {
				foreach(var submesh in Submeshes) {
					foreach(var triangle in submesh.Triangles) {
						yield return triangle;
					}
				}
			}
		}

		public bool IsUsingUvChannel(int channel) => (descriptor.uvMask & (1 << channel)) != 0;

		/// <summary>
		/// Regenerate vertex normals using their real geometry.
		/// </summary>
		public void RecalculateNormals() {
			Dictionary<Vertex, List<Vector3>> normalRecords = new(Vertices.Select(
				v => new KeyValuePair<Vertex, List<Vector3>>(v, new List<Vector3>())
			));
			foreach(var triangle in Triangles) {
				var normal = triangle.Normal;
				foreach(var vertex in triangle.Vertices)
					normalRecords[vertex].Add(normal);
			}
			foreach(var (vertex, normals) in normalRecords) {
				if(normals.Count == 0) {
					vertex.normal = default;
					continue;
				}
				vertex.normal = normals.Aggregate((a, b) => a + b) / normals.Count;
			}
			descriptor.useNormals = true;
		}

		public bool IsPointInside(in Vector3 point) {
			var raycastResults = RaycastAll(new Ray(point, Random.insideUnitSphere));
			// TODO: Consider the edge cases.
			return raycastResults.Length % 2 == 1;
		}

		public struct RaycastResult {
			public Triangle triangle;
			public Vector3 point;
		}

		public RaycastResult[] RaycastAll(in Ray ray) {
			List<RaycastResult> results = new();
			foreach(var triangle in Triangles) {
				var localToCut = triangle.MeshToTriangleMatrix;

				if(!RayCastOnXYPlane(
					localToCut.MultiplyPoint(ray.origin),
					localToCut.MultiplyVector(ray.direction),
					out Vector3 cutPoint,
					out float t
				))
					continue;
				if(t < 0 || cutPoint.x < 0 || cutPoint.y < 0 || cutPoint.x + cutPoint.y > 1) {
					continue;
				}
				results.Add(new() {
					point = localToCut.inverse.MultiplyPoint(cutPoint),
					triangle = triangle,
				});
			}
			return results.ToArray();
		}

		public void Transform(in Matrix4x4 transformation) {
			foreach(var vertex in Vertices) {
				vertex.Transform(transformation);
			}
		}

		/// <summary>
		/// See the non-procedural version of this function.
		/// </summary>
		/// <remarks>
		/// This method will stop and enumerate a list of triangles every time
		/// when a new batch of triangles are cut out of an external triangle.
		/// You can modify the triangle list to decide which of them are kept.
		/// </remarks>
		public IEnumerable<List<Triangle>> CutProcedural(MeshEx cut, int maxIteration) {
			foreach(var submesh in Submeshes) {
				// The triangles of this submesh will be modified on the fly,
				// so we need to make a cache list of them before doing the job.
				// Newly-cut triangles will be added into this list and processed
				// in later iterations.
				HashSet<Triangle> targetTriangles = new(submesh.Triangles);
				for(int iterationCount = 0; targetTriangles.Count > 0;) {
					if(++iterationCount > maxIteration) {
						Debug.LogError("We are stuck in an infinite loop while cutting mesh-exes.");
						break;
					}
					List<Triangle> processedTargets = new(), newTargets = new();
					foreach(var targetTriangle in targetTriangles) {
						processedTargets.Add(targetTriangle);
						if(!CutTriangleWithMeshEx(targetTriangle, cut, out Triangle[] cutTriangles))
							continue;
						submesh.RemoveTriangle(targetTriangle);
						newTargets.AddRange(cutTriangles);
					}
					yield return newTargets;
					foreach(var processed in processedTargets)
						targetTriangles.Remove(processed);
					foreach(var newTarget in newTargets) {
						submesh.AddTriangle(newTarget);
						targetTriangles.Add(newTarget);
					}
				}
			}
		}

		/// <summary>
		/// Cut this mesh by every triangles in another mesh in place.
		/// </summary>
		public void Cut(MeshEx cut, int maxIteration = defaultCutIteration) {
			if(useCutDebug)
				maxIteration = debugMaxCutIteration;
			foreach(var _ in CutProcedural(cut, maxIteration)) ;
		}

		/// <summary>
		/// Add a submesh by reference.
		/// </summary>
		/// <remarks>
		/// Note that the submeshes are added by reference.
		/// If this submesh is modified elsewhere, the changes will also effect here.
		/// </remarks>
		public bool AddSubmesh(Submesh submesh) {
			if(submeshes.Contains(submesh))
				return false;
			submeshes.Add(submesh);
			return true;
		}

		public Submesh CreateSubmesh() {
			Submesh submesh = new();
			submeshes.Add(submesh);
			return submesh;
		}

		public void JoinMeshEx(MeshEx meshEx) {
			foreach(var submesh in meshEx.Submeshes) {
				AddSubmesh(submesh);
			}
		}

		public int RemoveTrianglesWhere(System.Func<Triangle, bool> predicate) {
			int count = 0;
			foreach(var submesh in Submeshes) {
				List<Triangle> removingTriangles = new();
				foreach(var triangle in submesh.Triangles) {
					if(predicate(triangle))
						removingTriangles.Add(triangle);
				}
				foreach(var triangle in removingTriangles) {
					submesh.RemoveTriangle(triangle);
				}
				count += submesh.Triangles.Count;
			}
			return count;
		}
		#endregion

		#region Functions
		/// <summary>
		/// Cast a ray on the plane of z=0 and see where it lands.
		/// </summary>
		/// <param name="point">The point on the XY plane.</param>
		/// <param name="t">The position of the point on the ray.</param>
		/// <returns>Whether it lands or not.</returns>
		private static bool RayCastOnXYPlane(Vector3 origin, Vector3 direction, out Vector3 point, out float t) {
			if(direction.z == 0) {
				point = default;
				t = default;
				return false;
			}
			t = -(origin.z / direction.z);
			point = origin + t * direction;
			return true;
		}

		/// <summary>
		/// Cast one ray in 2D space on another and see where it lands.
		/// </summary>
		/// <param name="a">The start point of the ray.</param>
		/// <param name="b">The end point of the ray.</param>
		/// <param name="ray">The other (casting) ray.</param>
		/// <param name="point">The point that it casts to.</param>
		/// <param name="t">The portion from a to b of the landing point.</param>
		/// <returns></returns>
		private static bool TwoRayCast2D(Vector2 a, Vector2 b, Ray2D ray, out Vector2 point, out float t) {
			Vector2 axis = b - a, axisNorm = axis.normalized;
			Vector2 perpendicular = Perpendicular2D(a - ray.origin, axisNorm);
			var distance = perpendicular.magnitude;
			if(distance == 0) {
				point = default;
				t = Mathf.Infinity;
				return false;
			}
			// `ray.direction` should be normalized.
			point = ray.origin + ray.direction * distance;
			t = Vector2.Dot(point - a, axisNorm) / axis.magnitude;
			return true;
		}

		private static Vector2 Perpendicular2D(Vector2 vector, Vector2 to) {
			to = to.normalized;
			return vector - Vector2.Dot(vector, to) * to;
		}

		private static bool InRangeInclusive(float v, float min, float max) {
			return v >= min && v <= max;
		}

		private static bool CutTriangleWithMeshEx(Triangle triangle, MeshEx meshEx, out Triangle[] results) {
			List<Triangle> targetTriangles = new() { triangle };
			bool hasCutDuringIteration = false;
			foreach(var cuttingTriangle in meshEx.Triangles) {
				List<Triangle> currentTargets = new(targetTriangles);
				targetTriangles.Clear();
				foreach(var targetTriangle in currentTargets) {
					if(!targetTriangle.Cut(cuttingTriangle, out Triangle[] cutResults))
						continue;
					hasCutDuringIteration |= true;
					targetTriangles.AddRange(cutResults);
				}
			}
			if(!hasCutDuringIteration) {
				results = null;
				return false;
			}
			results = targetTriangles.ToArray();
			return true;
		}
		#endregion
	}
}