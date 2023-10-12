using UnityEngine;

namespace NaniCore.Loopool {
	public class UvVisualizer : MonoBehaviour {
		#region Serialzied fields
		public bool onScreenSpace = false;
		#endregion

		#region Functions
		private void OnPostFrameRender(Camera camera, RenderTexture cameraOutput) {
			string shaderName = !onScreenSpace ? "NaniCore/VisualizeUv" : "NaniCore/MeshUvToScreenUv";
			var mat = RenderUtility.GetPooledMaterial(shaderName);
			if(onScreenSpace) {
				mat.SetFloat("_Fov", camera.fieldOfView);
			}
			cameraOutput.RenderObject(gameObject, camera, mat);
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