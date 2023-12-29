using UnityEngine;

namespace NaniCore {
	public static class PhysicsUtility {
		/// <summary>
		/// Perform a fine-tuned sweep test on a rigidbody.
		/// </summary>
		/// <param name="distance">The distance of the sweep.</param>
		/// <param name="backupRatio">At what ratio on the sweep vector should we start with.</param>
		/// <param name="offset">The pre-applied offset before the sweep.</param>
		/// <returns></returns>
		public static bool SweepTestEx(this Rigidbody rb, Vector3 direction, out RaycastHit hit, float distance, float backupRatio, Vector3 offset, LayerMask layerMask) {
			direction.Normalize();
			var originalPos = rb.position;
			rb.position += direction * (distance * backupRatio * -1) + offset;
			RaycastHit[] hits = rb.SweepTestAll(direction, distance);
			rb.position = originalPos;
#if UNITY_2022_3_OR_NEWER
			// Rigidbody.excludeLayers were not supported until Unity 2022.
			layerMask = layerMask & ~rb.excludeLayers;
#endif
			foreach(var currentHit in hits) {
				int objectLayerMask = 1 << currentHit.collider.gameObject.layer;
				if((objectLayerMask & layerMask) == 0)
					continue;
				hit = currentHit;
				return true;
			}
			hit = default;
			return false;
		}
	}
}