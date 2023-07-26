using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace NaniCore.UnityPlayground {
	/*
	 * Here implements how a portal is rendered.
	 */
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
		public RenderSettings renderSettings;
		#endregion

		#region Fields
		private Material material;
		private RenderTexture displayTexture, unskewedTexture;
		private Camera viewCamera;
		#endregion

		#region Functions
		protected void SkewTexture() {
			if(twin == null)
				return;

			Camera camera = twin.viewCamera;
			RenderSettings.UvRectAnchor anchors = twin.renderSettings.uvAnchors;

			Vector3 bottomLeft = camera.WorldToViewportPoint(anchors.bottomLeft.position);
			Vector3 bottomRight = camera.WorldToViewportPoint(anchors.bottomRight.position);
			Vector3 topLeft = camera.WorldToViewportPoint(anchors.topLeft.position);
			Vector3 topRight = camera.WorldToViewportPoint(anchors.topRight.position);

			projectiveTransformMaterial.SetVector("_UvBottomLeft", bottomLeft);
			projectiveTransformMaterial.SetVector("_UvBottomRight", bottomRight);
			projectiveTransformMaterial.SetVector("_UvTopLeft", topLeft);
			projectiveTransformMaterial.SetVector("_UvTopRight", topRight);

			Graphics.Blit(unskewedTexture, displayTexture, projectiveTransformMaterial);
		}

		private void GlobalOnPostRenderCallback(Camera camera) {
			if(camera == viewCamera)
				OnViewCameraPostRender();
		}

		protected void OnViewCameraPostRender() {
			twin?.SkewTexture();
		}

		protected void InitializeRendering() {
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

		protected void FinalizeRendering() {
			Camera.onPostRender -= GlobalOnPostRenderCallback;
			RenderTexture.ReleaseTemporary(unskewedTexture);
			RenderTexture.ReleaseTemporary(displayTexture);
		}

		protected void UpdateRendering() {
			// Checks if view camera's target texture is twin's target texture.
			// If not, try set it.
			if(twin != null) {
				if(viewCamera.targetTexture != twin.unskewedTexture)
					viewCamera.targetTexture = twin.unskewedTexture;
			}

			// Sync FOV to main camera.
			if(Camera.main) {
				viewCamera.fieldOfView = Camera.main.fieldOfView;
			}
		}

		protected void LateUpdateRendering() {
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
		#endregion
	}
}