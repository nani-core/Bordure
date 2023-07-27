using UnityEngine;
using System.Collections.Generic;
using System;

namespace NaniCore.UnityPlayground {
	/*
	 * Here implements how a portal controls its passengers -- that is,
	 * the objects that are passing through it.
	 */
	[RequireComponent(typeof(Collider))]
	public partial class Portal : MonoBehaviour {
		#region Passenger definitions
		public abstract class Passenger : IDisposable {
			public readonly Collider root;
			public readonly Transform transform;
			public Mesh originalMesh, clampedMesh;
			protected Matrix4x4 lastRelativeMatrix;

			public Passenger(Collider root, Transform transform, Mesh originalMesh) {
				this.root = root;
				this.transform = transform;
				this.originalMesh = originalMesh;
				clampedMesh = Instantiate(originalMesh);
				clampedMesh.name = $"{originalMesh.name} (portal- clampped instance)";
			}

			public void ClampMeshToPortal(Portal portal) {
				if(originalMesh == null)
					return;

				Vector3[] clampedVertices = originalMesh.vertices;

				Matrix4x4 localToPortal = portal.transform.worldToLocalMatrix * transform.localToWorldMatrix;
				Matrix4x4 portalToLocal = localToPortal.inverse;

				for(int i = 0; i < clampedVertices.Length; ++i) {
					Vector3 positionUnderPortal = localToPortal.MultiplyPoint(clampedVertices[i]);
					if(positionUnderPortal.z > 0) {
						positionUnderPortal.z = 0;
						Vector3 restoredLocalPosition = portalToLocal.MultiplyPoint(positionUnderPortal);
						restoredLocalPosition.x = 0;
						restoredLocalPosition.y = 0;
						clampedVertices[i] = restoredLocalPosition;
					}
				}

				clampedMesh.vertices = clampedVertices;
			}

			public abstract void Dispose();

			public bool ReclampMeshIfNecessary(Portal portal) {
				Matrix4x4 relativeMatrix = portal.transform.worldToLocalMatrix * transform.localToWorldMatrix;
				if(lastRelativeMatrix == relativeMatrix)
					return false;
				ClampMeshToPortal(portal);
				lastRelativeMatrix = relativeMatrix;
				return true;
			}
		}

		public class MeshFilterPassenger : Passenger {
			public readonly MeshFilter meshFilter;

			public MeshFilterPassenger(Collider root, MeshFilter meshFilter)
				: base(root, meshFilter.transform, meshFilter.sharedMesh) {
				this.meshFilter = meshFilter;
				meshFilter.sharedMesh = clampedMesh;
			}

			public override void Dispose() {
				if(meshFilter != null)
					meshFilter.sharedMesh = originalMesh;
			}
		}

		public class MeshColliderPassenger : Passenger {
			public readonly MeshCollider meshCollider;

			public MeshColliderPassenger(Collider root, MeshCollider meshCollider)
				: base(root, meshCollider.transform, meshCollider.sharedMesh) {
				this.meshCollider = meshCollider;
				meshCollider.sharedMesh = clampedMesh;
			}

			public override void Dispose() {
				if(meshCollider != null)
					meshCollider.sharedMesh = originalMesh;
			}
		}
		#endregion

		#region Fields
		protected Dictionary<Collider, HashSet<Passenger>> rootedPassengers = new Dictionary<Collider, HashSet<Passenger>>();
		#endregion

		#region Functions
		protected HashSet<Passenger> TryAddRoot(Collider collider, out string error) {
			error = null;

			// Check if collider is already recorded as root.
			if(rootedPassengers.ContainsKey(collider)) {
				error = "Root already recorded.";
				return rootedPassengers[collider];
			}

			// Check if collider is a child of any recorded root.
			foreach(Collider root in rootedPassengers.Keys) {
				if(collider.transform.IsChildOf(root.transform)) {
					error = $"Is child of an existing root {root.name}";
					return rootedPassengers[root];
				}
			}

			// Admit the collider as a new root and create a set for it.
			HashSet<Passenger> set = new HashSet<Passenger>();
			rootedPassengers.Add(collider, set);
			return set;
		}

		protected void TryRemoveRoot(Collider collider) {
			if(!rootedPassengers.ContainsKey(collider))
				return;

			// Restore all the root's children passengers' mesh.
			foreach(Passenger passenger in rootedPassengers[collider])
				passenger.Dispose();

			// Remove the record to the root itself.
			rootedPassengers.Remove(collider);
		}
		#endregion

		#region Life cycle
		protected void OnColliderEnterPortal(Collider collider) {
			HashSet<Passenger> set = TryAddRoot(collider, out string addError);

			if(!string.IsNullOrEmpty(addError))
				Debug.LogWarning($"{collider.name} enters portal {name}, but is not recorded as root: {addError}", this);
			else
				Debug.Log($"{collider.name} enters portal {name}.", this);

			foreach(MeshFilter meshFilter in collider.GetComponentsInChildren<MeshFilter>(true))
				set.Add(new MeshFilterPassenger(collider, meshFilter));

			foreach(MeshCollider meshCollider in collider.GetComponentsInChildren<MeshCollider>(true))
				set.Add(new MeshColliderPassenger(collider, meshCollider));
		}

		protected void OnColliderExitPortal(Collider collider) {
			Debug.Log($"{collider.name} exits portal {name}.", this);
			TryRemoveRoot(collider);
		}

		protected void LastUpdatePassenger() {
			// For every tracked passenger, clamp its mesh if it has moved
			// relatively to portal.
			foreach(HashSet<Passenger> set in rootedPassengers.Values) {
				foreach(Passenger passenger in set) {
					passenger.ReclampMeshIfNecessary(this);
				}
			}
		}
		#endregion
	}
}