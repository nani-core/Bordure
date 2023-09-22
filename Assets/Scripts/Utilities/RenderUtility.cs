using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace NaniCore {
	public static class RenderUtility {
		private static Dictionary<string, Shader> shaderPool = new Dictionary<string, Shader>();
		public static Shader GetPoolShader(string name) {
			Shader shader = null;
			if(shaderPool.ContainsKey(name))
				shader = shaderPool[name];
			if(!(shader != null && shader.name == name))
				shader = Shader.Find(name);
			return shader;
		}

		private static Dictionary<string, Material> materialPool = new Dictionary<string, Material>();
		public static Material GetPooledMaterial(string shaderName) {
			if(materialPool.ContainsKey(shaderName)) {
				var pooledMat = materialPool[shaderName];
				if(pooledMat != null && pooledMat.shader?.name == shaderName)
					return pooledMat;
			}
			Shader shader = GetPoolShader(shaderName);
			if(shader == null)
				return null;
			Material mat = new Material(shader);
			materialPool.Add(shaderName, mat);
			return mat;
		}

		public static Material CreateIndependentMaterial(string shaderName) {
			var shader = GetPooledMaterial(shaderName);
			if(shader == null)
				return null;
			return new Material(shader);
		}

		public static void SetValue(this RenderTexture texture, Color value) {
			var temp = RenderTexture.GetTemporary(texture.descriptor);
			var mat = CreateIndependentMaterial("NaniCore/SetValue");
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

		public static void ReplaceByValue(this RenderTexture texture, Color value, RenderTexture replacement) {
			var rt = RenderTexture.GetTemporary(texture.descriptor);
			var mat = CreateIndependentMaterial("NaniCore/ReplaceByValue");
			mat.SetColor("_Value", value);
			mat.SetTexture("_ReplaceTex", replacement);
			Graphics.Blit(texture, rt, mat);
			Object.Destroy(mat);
			Graphics.Blit(rt, texture);
			RenderTexture.ReleaseTemporary(rt);
		}

		public static void IndicateByValue(this RenderTexture texture, Color value) {
			var temp = RenderTexture.GetTemporary(texture.descriptor);
			var mat = CreateIndependentMaterial("NaniCore/IndicateByValue");
			mat.SetColor("_Value", value);
			Graphics.Blit(texture, temp, mat);
			Object.Destroy(mat);
			Graphics.Blit(temp, texture);
			RenderTexture.ReleaseTemporary(temp);
		}
	}
}