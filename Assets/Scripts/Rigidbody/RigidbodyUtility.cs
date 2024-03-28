using UnityEngine;

namespace NaniCore.Bordure {
	public static class RigidbodyUtility {
		public static bool IsDerivedFromTier(this RigidbodyTier self, RigidbodyTier parent) {
			if((uint)self < (uint)parent)
				return false;
			return ((uint)self & (uint)parent) == (uint)parent;
		}

		public static bool IsOfTier(this RigidbodyAgent rba, RigidbodyTier tier) {
			if(rba == null)
				return false;
			return rba.Tier.IsDerivedFromTier(tier);
		}
		public static bool IsOfTier(this Rigidbody rb, RigidbodyTier tier) {
			return rb?.GetComponent<RigidbodyAgent>().IsOfTier(tier) ?? false;
		}


		public static RigidbodyTier GetTier(this RigidbodyAgent rba) {
			return rba?.Tier ?? RigidbodyTier.Default;
		}
		public static RigidbodyTier GetTier(this Rigidbody rb) {
			return rb?.GetComponent<RigidbodyAgent>()?.GetTier() ?? RigidbodyTier.Default;
		}
	}
}
