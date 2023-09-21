using UnityEngine;

namespace NaniCore {
	public static class RenderUtility {
		private const string setValueShaderName = "NaniCore/SetValue";
		private static Shader setValueShader;
		private static Shader SetValueShader {
			get {
				if(setValueShader == null)
					setValueShader = Shader.Find(setValueShaderName);
				return setValueShader;
			}
		}

		public static void SetValue(this RenderTexture texture, Color value) {
			var temp = RenderTexture.GetTemporary(texture.descriptor);
			var mat = new Material(SetValueShader);
			mat.SetColor("_Value", value);
			Graphics.Blit(texture, temp, mat);
			Object.Destroy(mat);
			Graphics.Blit(temp, texture);
			RenderTexture.ReleaseTemporary(temp);
		}

		public static void RenderObject(this RenderTexture texture, GameObject gameObject, Camera camera, Material material, int pass = 0) {
			if(material == null || texture == null)
				return;
			RenderObject(texture.colorBuffer, texture.depthBuffer, gameObject, camera, material, pass);
		}

		public static void RenderObject(RenderBuffer colorBuffer, RenderBuffer depthBuffer, GameObject gameObject, Camera camera, Material material, int pass = 0) {
			if(material == null)
				return;

			Graphics.SetRenderTarget(colorBuffer, depthBuffer);
			material.SetPass(pass);

			foreach(MeshFilter filter in gameObject.transform.GetComponentsInChildren<MeshFilter>()) {
				if(!(filter.GetComponent<MeshRenderer>()?.enabled ?? false))
					continue;
				var mesh = filter.sharedMesh;
				if(mesh == null)
					continue;

				Graphics.DrawMeshNow(mesh, filter.transform.localToWorldMatrix);
			}
		}
	}
}