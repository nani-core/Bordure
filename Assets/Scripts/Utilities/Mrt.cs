using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore.Loopool {
	public class Mrt : MonoBehaviour {
		#region Serialized fields
		public Color mrtValue = Color.white;
		#endregion

		#region Fields
		private IEnumerable<Renderer> renderers;
		private Material mrtMaterial;
		[SerializeField] private RenderTexture mrtTexture;
		[SerializeField] private RenderTexture maskedTexture;
		#endregion

		#region Properties
		public Renderer Renderer => Renderer;

		private Material MrtMaterial {
			get {
				if(mrtMaterial != null)
					return mrtMaterial;
				return mrtMaterial = RenderUtility.CreateIndependentMaterial("NaniCore/MRT");
			}
		}

		public RenderTexture MrtTexture => mrtTexture;
		public RenderTexture MaskedTexture => maskedTexture;

		public bool IsVisible {
			get => renderers.Any(renderer => renderer.enabled && renderer.isVisible);
		}
		#endregion

		#region Functions
		private void OnRendered(Camera camera, RenderTexture cameraOutput) {
			if(!IsVisible)
				return;
			mrtTexture.SetValue(Color.clear);
			MrtMaterial.SetColor("_Value", mrtValue);
			mrtTexture.RenderObject(gameObject, camera, MrtMaterial);
			Graphics.Blit(mrtTexture, maskedTexture);
			maskedTexture.ReplaceTextureByValue(mrtValue, cameraOutput);
		}

		public void Stamp(Camera camera) {
			if(camera == null)
				return;

			var uvTex = RenderUtility.CreateScreenSizedRT(RenderTextureFormat.ARGBFloat);
			Material uvMat = RenderUtility.GetPooledMaterial("NaniCore/MeshUvToScreenUv");
			uvMat.SetFloat("_Fov", camera.fieldOfView);
			uvTex.RenderObject(gameObject, camera, uvMat);

			var handler = gameObject.EnsureComponent<StampHandler>();
			handler.Initialize();

			RenderTexture stampingTexture = MaskedTexture.Duplicate();
			stampingTexture.UvMap(uvTex);
			// stampingTexture will be released properly by the handler on next
			// texture set or application quit.
			handler.SetStampingTexture(stampingTexture);

			uvTex.Destroy();
		}
		#endregion

		#region Life cycle
		protected void Start() {
			renderers = GetComponentsInChildren<Renderer>();
			mrtValue = Random.ColorHSV(0f, 1f, .4f, .6f, .2f, .8f);
		}

		protected void OnDestroy() {
			if(mrtMaterial != null)
				Destroy(mrtMaterial);
		}

		protected void OnEnable() {
			mrtTexture = RenderUtility.CreateScreenSizedRT();
			maskedTexture = RenderUtility.CreateScreenSizedRT();
			if(MainCamera.Instance)
				MainCamera.Instance.onRendered += OnRendered;
		}

		protected void OnDisable() {
			if(MainCamera.Instance)
				MainCamera.Instance.onRendered -= OnRendered;
			mrtTexture.Destroy();
			mrtTexture = null;
			maskedTexture.Destroy();
			maskedTexture = null;
		}
		#endregion
	}
}