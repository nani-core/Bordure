using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace NaniCore.UnityPlayground {
	public partial class Portal : MonoBehaviour {
		const string targetTexturePropertyName = "_MainTex";
		const string portalLayerName = "Portal";

		const string projectiveTransformShaderName = "Utility/Projective Transform by Corners";
		static Shader projectiveTransformShader;
		static Material projectiveTransformMaterial;
		static LayerMask viewCameraLayerMask;

		[Serializable]
		public class RenderSettings {
			public MeshRenderer renderer;
			public int meshIndex = 0;

			[Serializable]
			public struct UvRectAnchor {
				public Transform bottomLeft, bottomRight, topLeft, topRight;
			}
			public UvRectAnchor uvAnchors;
		}

		#region Serialized fields
		public Portal twin;
		public RenderSettings renderSettings;
		#endregion

		#region Fields
		private Material material;
		private RenderTexture displayTexture, unskewedTexture;
		private Camera viewCamera;
		#endregion

		#region Functions
		#region Display texture
		/// <summary>
		/// Apply perspective twist on display texture based on the relative
		/// transform of the main camera to twin.
		/// </summary>
		protected void SkewTexture() {
			if(twin == null || Camera.main == null)
				return;

			Camera camera = Camera.main;
			RenderSettings.UvRectAnchor anchors = renderSettings.uvAnchors;

			Vector2 bottomLeft = camera.WorldToViewportPoint(anchors.bottomLeft.position);
			Vector2 bottomRight = camera.WorldToViewportPoint(anchors.bottomRight.position);
			Vector2 topLeft = camera.WorldToViewportPoint(anchors.topLeft.position);
			Vector2 topRight = camera.WorldToViewportPoint(anchors.topRight.position);

			projectiveTransformMaterial.SetVector("_UvBottomLeft", bottomLeft);
			projectiveTransformMaterial.SetVector("_UvBottomRight", bottomRight);
			projectiveTransformMaterial.SetVector("_UvTopLeft", topLeft);
			projectiveTransformMaterial.SetVector("_UvTopRight", topRight);

			Graphics.Blit(unskewedTexture, displayTexture, projectiveTransformMaterial);
		}
		#endregion

		#region Custom events
		private void GlobalOnPostRenderCallback(Camera camera) {
			if(camera == viewCamera)
				OnViewCameraPostRender();
		}

		protected void OnViewCameraPostRender() {
			twin?.SkewTexture();
		}
		#endregion
		#endregion

		#region Life cycle
		protected void Awake() {
			projectiveTransformShader = Shader.Find(projectiveTransformShaderName);
			projectiveTransformMaterial = new Material(projectiveTransformShader);

			viewCameraLayerMask = ~0 & ~LayerMask.GetMask(portalLayerName);
		}

		protected void Start() {
			// Create a copy from the template material and apply it.
			// See https://docs.unity3d.com/ScriptReference/Renderer-materials.html for details
			// on how Unity instantiates materials.
			material = renderSettings.renderer.materials[renderSettings.meshIndex];
			material.name += " (instance)";

			unskewedTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);
			displayTexture = RenderTexture.GetTemporary(unskewedTexture.descriptor);
			material.SetTexture(targetTexturePropertyName, displayTexture);

			// Create a view camera for rendering twin's viewport.
			viewCamera = new GameObject("Portal Camera").AddComponent<Camera>();
#if UNITY_EDITOR
			SceneVisibilityManager.instance.DisablePicking(viewCamera.gameObject, false);
#endif
			viewCamera.transform.SetParent(transform, false);
			viewCamera.backgroundColor = Color.black;
			viewCamera.cullingMask = viewCameraLayerMask;
			Camera.onPostRender += GlobalOnPostRenderCallback;
		}

		protected void OnDestroy() {
			Camera.onPostRender -= GlobalOnPostRenderCallback;
			RenderTexture.ReleaseTemporary(unskewedTexture);
			RenderTexture.ReleaseTemporary(displayTexture);
		}

		protected void Update() {
			// Checks if view camera's target texture is twin's target texture.
			// If not, try set it.
			if(twin != null) {
				if(viewCamera.targetTexture != twin.unskewedTexture)
					viewCamera.targetTexture = twin.unskewedTexture;
			}
		}

		protected void LateUpdate() {
			// Updates twin's view camera's transform based on the relative transform
			// between the main camera and self.
			if(Camera.main != null && twin != null) {
				Transform twinCamera = twin.viewCamera.transform;
				Transform mainCamera = Camera.main.transform;
				Matrix4x4 worldToLocal = transform.worldToLocalMatrix;
				twinCamera.localPosition = worldToLocal.MultiplyPoint(mainCamera.position);
				twinCamera.localRotation = worldToLocal.rotation * mainCamera.rotation;
			}
		}

		protected void OnBecameVisible() => enabled = true;
		protected void OnBecameInvisible() => enabled = false;
		#endregion
	}
}