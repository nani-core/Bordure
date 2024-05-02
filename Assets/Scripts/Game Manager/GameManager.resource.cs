using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public partial class GameManager {
		private List<System.WeakReference<Object>> temporaryResources = new();

		public void RegisterTemporaryResource(Object resource) {
			if(resource == null)
				return;
			temporaryResources.Add(new(resource));
		}

		public void ReleaseAllTemporaryResources() {
			foreach(var reference in temporaryResources) {
				if(!reference.TryGetTarget(out var resource))
					continue;
				if(resource == null)
					continue;
#if DEBUG && false
				Debug.Log($"Release temporary resource {resource}");
#endif
				ReleaseResource(resource);
			}
			temporaryResources.Clear();
		}

		public void ReleaseResource(Object resource) {
			if(resource == null)
				return;
			switch(resource) {
				case RenderTexture rt:
					RenderUtility.Destroy(rt);
					break;
				default:
					Destroy(resource);
					break;
			}
		}
	}
}