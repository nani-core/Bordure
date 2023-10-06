using UnityEngine;
using System.Linq;

namespace NaniCore.Loopool {
	public class Mrt : MonoBehaviour {
		#region Serialized fields
		public Color mrtValue = Color.white;
		#endregion

		#region Fields
		private Material mrtMaterial;
		private RenderTexture mrtTexture;
		public RenderTexture maskedTexture;
		#endregion

		#region Properties
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
			get => GetComponentsInChildren<Renderer>().Any(r => r.enabled && r.isVisible);
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
		#endregion

		#region Life cycle
		protected void Start() {
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