using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public partial class GameManager : MonoBehaviour {
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
				Debug.Log($"Release temporary resource {resource}");
				ReleaseResource(resource);
			}
			temporaryResources.Clear();
		}

		private static void ReleaseResource(Object resource) {
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