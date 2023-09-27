using UnityEngine;
using UnityEngine.Rendering;
using System;

namespace NaniCore.Loopool {
	[RequireComponent(typeof(Camera))]
	public class MainCamera : MonoBehaviour {
		#region Static
		private static MainCamera instance;
		public static MainCamera Instance {
			get {
				if(instance == null)
					instance = FindObjectOfType<MainCamera>();
				return instance;
			}
		}
		#endregion

		#region Fields
		private new Camera camera;
		private RenderTexture outputTexture;
		public Action<Camera, RenderTexture> onRendered;
		#endregion

		#region Fields
		public Camera Camera => camera;
		#endregion

		#region Functions
		private void OnBeginRenderCallback(ScriptableRenderContext content, Camera camera) {
			if(camera != this.camera)
				return;
			// For unknown reasons, we can't just set targetTexture once at the beginning.
			// Doing that will cause the camera to not render at all.
			camera.targetTexture = outputTexture;
		}
		private void OnEndRenderCallback(ScriptableRenderContext content, Camera camera) {
			if(camera != this.camera)
				return;

			try {
				onRendered?.Invoke(camera, outputTexture);
			}
			catch(System.Exception e) {
				Debug.LogError(e, this);
			}

			Graphics.Blit(outputTexture, null as RenderTexture);
			camera.targetTexture = null;
		}
		#endregion

		#region Life cycle
		protected void Start() {
			camera = GetComponent<Camera>();
		}

		protected void OnEnable() {
			outputTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);
			RenderPipelineManager.beginCameraRendering += OnBeginRenderCallback;
			RenderPipelineManager.endCameraRendering += OnEndRenderCallback;
		}

		protected void OnDisable() {
			RenderPipelineManager.beginCameraRendering -= OnBeginRenderCallback;
			RenderPipelineManager.endCameraRendering -= OnEndRenderCallback;
			RenderTexture.ReleaseTemporary(outputTexture);
			outputTexture = null;
		}
		#endregion
	}
}