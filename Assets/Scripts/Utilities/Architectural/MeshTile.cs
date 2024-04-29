using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore {
	using MeshEx = Nianyi.MeshEx;
	using Bordure;

	public class MeshTile : ArchitectureGenerator {
		#region Serialized fields
		public GameObject[] tiles = new GameObject[0];
		public Vector3 i = Vector3.right, j = Vector3.up, k = Vector3.forward;
		public Vector3Int count = Vector3Int.one;
		public Vector3 uvw = Vector3.one * .5f;
		public List<GameObject> hollowObjects = new();
		public bool batch = true;
		#endregion

		#region Interfaces
		public GameObject[] UsableTiles => tiles.Where(tile => tile != null).ToArray();
		#endregion

		#region Functions
		protected override string GizmozRootName => "$MeshTileGizmosRoot";

		protected override void Construct(Transform under, Instantiator instantiator) {
			var usableTiles = UsableTiles;
			if(usableTiles.Length <= 0)
				return;

			if(under == null) {
				Debug.LogWarning("Base transform is null.");
			}

			List<GameObject> instances = new();
			for(int ix = 0; ix < count.x; ++ix) {
				for(int iy = 0; iy < count.y; ++iy) {
					for(int iz = 0; iz < count.z; ++iz) {
						var selfOffset = i * ix + j * iy + k * iz;
						var scale = i * uvw.x + j * uvw.y + k * uvw.z;
						Vector3 localPosition = selfOffset - Vector3.Scale(count - Vector3.one, scale);
						Vector3 worldPosition = under.localToWorldMatrix.MultiplyPoint(localPosition);
						if(hollowObjects.Any(o => CheckIfPointIsIn(worldPosition, o)))
							continue;

						var index = HashUtility.Hash(seed, ix, iy, iz) % usableTiles.Length;
						var tile = usableTiles[index];
						var instance = instantiator(tile, under).transform;
						instance.localPosition = localPosition;
						instance.gameObject.isStatic = gameObject.isStatic;
						instances.Add(instance.gameObject);
					}
				}
			}

			if(batch)
				BatchMeshes(under, instances);
		}

		private struct BatchInfo {
			public int submeshIndexOffset;
			public Mesh originalMesh;
			public MeshEx templateMeshEx;
		}

		private void BatchMeshes(Transform under, List<GameObject> targetObjects) {
			if(!Application.isPlaying)
				return;
			if(tiles.Length != 1)
				return;
			if(under == null)
				under = new GameObject().transform;

			// List mesh instances.
			List<(MeshFilter, MeshRenderer)> pairs = new();
			foreach(var targetObject in targetObjects) {
				foreach(var renderer in targetObject.GetComponentsInChildren<MeshRenderer>()) {
					if(!renderer.TryGetComponent(out MeshFilter filter))
						continue;
					Mesh mesh = filter.sharedMesh;
					if(mesh == null)
						continue;
					if(!mesh.isReadable) {
						Debug.LogWarning($"Warning: {mesh} is not CPU-readable, skipping batching.", mesh);
						continue;
					}
					pairs.Add((filter, renderer));
				}
			}
			if(pairs.Count == 0)
				return;
			
			// Sort out submesh information.
			Dictionary<Mesh, BatchInfo> batchInfos = new();
			List<Material> materials = new();
			int submeshIndexOffset = 0;
			foreach(var (filter, renderer) in pairs) {
				Mesh mesh = filter.sharedMesh;
				if(batchInfos.ContainsKey(mesh))
					continue;

				materials.AddRange(renderer.sharedMaterials);

				BatchInfo info = new() {
					submeshIndexOffset = submeshIndexOffset,
					originalMesh = mesh,
					templateMeshEx = new MeshEx(mesh),
				};
				batchInfos[mesh] = info;

				submeshIndexOffset += mesh.subMeshCount;
			}

			// Merge into one sum meshex.
			MeshEx sum = new();
			foreach(var (filter, renderer) in pairs) {
				if(!batchInfos.ContainsKey(filter.sharedMesh))
					continue;
				var info = batchInfos[filter.sharedMesh];
				AddToMeshEx(sum,
					info.templateMeshEx,
					info.submeshIndexOffset,
					MathUtility.RelativeTransform(filter.transform, under)
				);
			}

			// Destroy original objects.
			foreach(var targetObject in targetObjects)
				Destroy(targetObject);

			// Create the new mesh.
			Mesh sumMesh = sum.ToMesh();
			sumMesh.name = $"Batched Mesh ({string.Join(", ", batchInfos.Values.Select(i => i.originalMesh.name))})";
			GameManager.Instance.RegisterTemporaryResource(sumMesh);
			under.gameObject.AddComponent<MeshFilter>().sharedMesh = sumMesh;
			under.gameObject.AddComponent<MeshRenderer>().sharedMaterials = materials.ToArray();
			under.gameObject.AddComponent<MeshCollider>();

			// Generate the agent
			if(tiles[0].TryGetComponent<RigidbodyAgent>(out var agent)) {
				var newAgent = under.gameObject.AddComponent<RigidbodyAgent>();
				newAgent.Tier = agent.Tier;
			}
		}

		private static void AddToMeshEx(MeshEx destination, MeshEx add, int subMeshOffset, Matrix4x4 transformation) {
			// Guarantee enough submesh seats.
			int desiredSubmeshCount = subMeshOffset + add.Submeshes.Count;
			while(destination.Submeshes.Count < desiredSubmeshCount)
				destination.CreateSubmesh();

			MeshEx copy = new(add);
			copy.Transform(transformation);
			for(int i = 0; i < copy.Submeshes.Count; ++i) {
				var targetSubmesh = destination.Submeshes[i + subMeshOffset];
				foreach(var triangle in copy.Submeshes[i].Triangles)
					targetSubmesh.AddTriangle(triangle);
			}
		}

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
	}
}