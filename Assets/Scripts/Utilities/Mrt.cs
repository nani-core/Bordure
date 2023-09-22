using UnityEngine;

namespace NaniCore.Loopool {
	public class Mrt : MonoBehaviour {
		private const string mrtShaderName = "NaniCore/MRT";
		private static Shader mrtShader;

		#region Serialized fields
		public Color value = Color.white;
		#endregion

		#region Fields
		private Material mrtMaterial;
		#endregion

		#region Properties
		private Material MrtMaterial {
			get {
				if(mrtMaterial != null)
					return mrtMaterial;
				if(mrtShader == null)
					mrtShader = Shader.Find(mrtShaderName);
				mrtMaterial = new Material(mrtShader);
				return mrtMaterial;
			}
		}
		#endregion

		#region Functions
		public void RenderToTexture(RenderTexture texture) {
			MrtMaterial.SetColor("_Value", value);
			texture.RenderObject(gameObject, MrtMaterial);
		}
		#endregion

		#region Life cycle
		protected void Start() {
			value = Random.ColorHSV(0f, 1f, .4f, .6f, .2f, .8f);
		}

		protected void OnDestroy() {
			if(mrtMaterial != null)
				Destroy(mrtMaterial);
		}
		#endregion
	}
}