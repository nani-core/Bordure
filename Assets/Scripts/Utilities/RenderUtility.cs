using UnityEngine;
using UnityEngine.Rendering;

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

		public static void RenderObject(this RenderTexture texture, GameObject gameObject, Material material, int pass = 0) {
			if(material == null || texture == null)
				return;
			RenderObject(texture.colorBuffer, texture.depthBuffer, gameObject, material, pass);
		}

		public static void RenderObject(RenderBuffer colorBuffer, RenderBuffer depthBuffer, GameObject gameObject, Material material, int pass = 0) {
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

		public static void Copy(this RenderTexture texture, RenderTexture source) {
			var cb = new CommandBuffer();
			cb.CopyTexture(source, texture);
			Graphics.ExecuteCommandBuffer(cb);
			cb.Dispose();
		}

		private const string replaceByValueShaderName = "NaniCore/ReplaceByValue";
		private static Shader replaceByValueShader;
		private static Shader ReplaceByValueShader {
			get {
				if(replaceByValueShader == null)
					replaceByValueShader = Shader.Find(replaceByValueShaderName);
				return replaceByValueShader;
			}
		}
		public static void ReplaceByValue(this RenderTexture texture, Color value, RenderTexture replacement) {
			var mat = new Material(ReplaceByValueShader);
			mat.SetColor("_Value", value);
			mat.SetTexture("_ReplaceTex", replacement);
			var rt = RenderTexture.GetTemporary(texture.descriptor);
			Graphics.Blit(texture, rt, mat);
			Graphics.Blit(rt, texture);
			RenderTexture.ReleaseTemporary(rt);
			Object.Destroy(mat);
		}
	}
}