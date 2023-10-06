using UnityEngine;

namespace NaniCore.Loopool {
	public class UvVisualizer : MonoBehaviour {
		#region Functions
		private void OnPostFrameRender(Camera camera, RenderTexture cameraOutput) {
			cameraOutput.RenderObject(gameObject, camera, RenderUtility.GetPooledMaterial("NaniCore/VisualizeUv"));
		}
		#endregion

		#region Life cycle
		protected void OnEnable() {
			if(MainCamera.Instance)
				MainCamera.Instance.onRendered += OnPostFrameRender;
		}

		protected void OnDisable() {
			if(MainCamera.Instance)
				MainCamera.Instance.onRendered -= OnPostFrameRender;
		}
		#endregion
	}
}