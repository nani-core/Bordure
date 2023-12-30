using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore {
	public static class PhysicsUtility {
		public static List<RaycastHit> SweepTestAll(this Rigidbody rb, Vector3 direction, float distance, float backupRatio, Vector3 offset, LayerMask layerMask) {
			direction.Normalize();
			var originalPos = rb.position;
			rb.position += direction * (distance * backupRatio * -1) + offset;
			List<RaycastHit> hits = rb.SweepTestAll(direction, distance).ToList();
			rb.position = originalPos;
			// Sort the hits by normalized direction.
			hits.Sort((a, b) => {
				float da = a.distance * Mathf.Abs(Vector3.Dot(a.normal, direction));
				float db = b.distance * Mathf.Abs(Vector3.Dot(b.normal, direction));
				return (int)Mathf.Sign(da - db);
			});
			hits.RemoveAll(hit => {
				int objectLayerMask = 1 << hit.collider.gameObject.layer;
				return (objectLayerMask & layerMask) == 0;
			});
			return hits;
		}

		/// <summary>
		/// Perform a sweep test on a rigidbody.
		/// </summary>
		/// <param name="distance">The distance of the sweep.</param>
		/// <param name="backupRatio">At what ratio on the sweep vector should we start with.</param>
		/// <param name="offset">The pre-applied offset before the sweep.</param>
		/// <returns></returns>
		public static bool SweepTest(this Rigidbody rb, Vector3 direction, out RaycastHit hit, float distance, float backupRatio, Vector3 offset, LayerMask layerMask) {
			var hits = SweepTestAll(rb, direction, distance, backupRatio, offset, layerMask);
			if(hits.Count == 0) {
				hit = default;
				return false;
			}
			hit = hits[0];
			return true;
		}

		public static List<RaycastHit> RaycastAll(Vector3 origin, Vector3 direction, float distance, LayerMask layerMask, bool includeTriggers) {
			var hits = Physics.RaycastAll(origin, direction, distance, layerMask).ToList();
			if(!includeTriggers)
				hits.RemoveAll(hit => hit.collider.isTrigger);
			hits.Sort((a, b) => {
				return (int)Mathf.Sign(a.distance - b.distance);
			});
			return hits;
		}

		public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hit, float distance, LayerMask layerMask, bool includeTriggers) {
			direction.Normalize();
			var hits = RaycastAll(origin, direction, distance, layerMask, includeTriggers);
			if(hits.Count == 0) {
				hit = default;
				return false;
			}
			hit = hits[0];
			return true;
		}
	}
}