using UnityEngine;

namespace NaniCore.Loopool {
	public static class RigidbodyUtility {
		public static bool IsDerivedFromTier(this RigidbodyTier self, RigidbodyTier parent) {
			if(parent == 0)
				return false;
			if(self < parent)
				return false;
			return (self & parent) == parent;
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
			return rba?.Tier ?? RigidbodyTier.Undefined;
		}
		public static RigidbodyTier GetTier(this Rigidbody rb) {
			return rb?.GetComponent<RigidbodyAgent>()?.GetTier() ?? RigidbodyTier.Undefined;
		}
	}
}
