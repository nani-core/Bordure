using UnityEngine;
using System;

namespace NaniCore.Loopool {
	/// <summary>
	/// This is a auxiliary agent class that helps to manage the life cycle
	/// of a stamped gameobject.
	/// </summary>
	/// <remarks>
	///	Don't place manually in edit mode, should be managed by script instead.
	/// Call Initialize() before using any functions/properties.
	/// </remarks>
	[RequireComponent(typeof(Renderer))]
	public class StampHandler : MonoBehaviour {
		#region Fields
		private bool hasInitialized = false;
		private new Renderer renderer;
		private bool hasStamped = false;
		private Material originalMaterial;
		#endregion

		#region Properties
		public Material Material {
			get => renderer?.sharedMaterial;
			set {
				if(renderer == null || value == Material)
					return;
				if(hasStamped && renderer?.sharedMaterial) {
					ReleaseCurrentStampingTexture();
					Destroy(renderer.sharedMaterial);
				}
				renderer.sharedMaterial = value;
			}
		}
		#endregion

		#region Functions
		/// <summary>
		/// Initializes self.
		/// </summary>
		/// <remarks>
		/// It is adviced to manually call this function before any immediate
		/// logics, though this will be executed in Start().
		/// It is okay to call this function multiple times, only the first
		/// call would take effect.
		/// </remarks>
		public void Initialize() {
			if(hasInitialized)
				return;
			hasInitialized = true;

			renderer = GetComponent<Renderer>();
		}

		/// <summary>
		/// Records the current texture and marks self as stamped.
		/// </summary>
		public void SetStampedState() {
			if(hasStamped)
				return;
			originalMaterial = Material;

			Material = RenderUtility.CreateIndependentMaterial("HDRP/Unlit");
			Material.SetFloat("_AlphaCutoffEnable", 1f);
			Material.SetFloat("_AlphaCutoff", 1f);

			hasStamped = true;
		}

		/// <summary>
		/// Restore original material and release the stamping material.
		/// </summary>
		public void Restore() {
			if(!hasStamped)
				return;
			Material = originalMaterial;
		}

		/// <summary>
		/// Make material unlit (if not) and set its texture.
		/// </summary>
		/// <remarks>
		/// Original material will be recorded, and the newly created stamping
		/// unlit material will be automatically released on destroy.
		/// </remarks>
		public void SetStampingTexture(RenderTexture texture) {
			SetStampedState();
			ReleaseCurrentStampingTexture();
			Material.mainTexture = texture;
		}

		private void ReleaseCurrentStampingTexture() {
			if(Material?.mainTexture == null)
				return;

			var textureToBeDestroyed = Material.mainTexture;
			Material.mainTexture = null;
			switch(textureToBeDestroyed) {
				case RenderTexture rt:
					rt.Destroy();
					break;
				default:
					Destroy(textureToBeDestroyed);
					break;
			}
		}

		public static void Stamp(GameObject target, Camera camera) {
			if(camera == null)
				return;

			var uvTex = RenderUtility.CreateScreenSizedRT();
			Material uvMat = RenderUtility.GetPooledMaterial("NaniCore/MeshUvToScreenUv");
			uvMat.SetFloat("_Fov", camera.fieldOfView);
			uvTex.RenderObject(target, camera, uvMat);

			var handler = target.EnsureComponent<StampHandler>();
			handler.Initialize();

			var maskedTexture = RenderUtility.CreateScreenSizedRT();
			maskedTexture.RenderMask(target, camera, true); // Disregarding depth.
			if(false) {
				// Debug codes.
				handler.SetStampingTexture(uvTex);

				maskedTexture.Destroy();
				return;
			}
			{
				var capture = RenderUtility.CreateScreenSizedRT(RenderTextureFormat.Default);
				capture.Capture(camera);
				maskedTexture.ReplaceTextureByValue(Color.white, capture);
				capture.Destroy();
			}
			maskedTexture.UvMap(uvTex);
			// stampingTexture will be released properly by the handler on next
			// texture set or application quit.
			handler.SetStampingTexture(maskedTexture);

			uvTex.Destroy();
		}
		#endregion

		#region Life cycle
		protected void Start() {
			Initialize();
		}

		protected void OnDestroy() {
			Material = null;
		}
		#endregion
	}
}