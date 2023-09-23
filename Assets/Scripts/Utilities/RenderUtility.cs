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

		public static Vector2Int Size(this RenderTexture texture) {
			if(texture == null)
				return Vector2Int.zero;
			return new Vector2Int(texture.width, texture.height);
		}

		public static RenderTexture Duplicate(this RenderTexture texture, Material material = null) {
			if(texture == null)
				return null;
			var copy = RenderTexture.GetTemporary(texture.descriptor);
			if(material != null)
				Graphics.Blit(texture, copy, material);
			else
				Graphics.Blit(texture, copy);
			return copy;
		}

		public static void Destroy(this RenderTexture texture) {
			if(RenderTexture.active == texture)
				RenderTexture.active = null;
			RenderTexture.ReleaseTemporary(texture);
		}

		public static void Apply(this RenderTexture texture, Material material) {
			if(texture == null)
				return;
			var copy = texture.Duplicate(material);
			Graphics.Blit(copy, texture);
			copy.Destroy();
		}

		public static void SetValue(this RenderTexture texture, Color value) {
			var mat = GetPooledMaterial("NaniCore/SetValue");
			mat.SetColor("_Value", value);
			texture.Apply(mat);
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

		public static void CopyFrom(this RenderTexture texture, RenderTexture source) {
			var cb = new CommandBuffer();
			cb.CopyTexture(source, texture);
			Graphics.ExecuteCommandBuffer(cb);
			cb.Dispose();
		}

		public static void ReplaceTextureByValue(this RenderTexture texture, Color value, RenderTexture replacement) {
			var mat = GetPooledMaterial("NaniCore/ReplaceTextureByValue");
			mat.SetColor("_Value", value);
			mat.SetTexture("_ReplaceTex", replacement);
			texture.Apply(mat);
		}

		public static void ReplaceValueByValue(this RenderTexture texture, Color value, Color replacement) {
			var mat = GetPooledMaterial("NaniCore/ReplaceValueByValue");
			mat.SetColor("_Value", value);
			mat.SetColor("_ReplaceValue", replacement);
			texture.Apply(mat);
		}

		public static void IndicateByValue(this RenderTexture texture, Color value) {
			var mat = GetPooledMaterial("NaniCore/IndicateByValue");
			mat.SetColor("_Value", value);
			texture.Apply(mat);
		}

		/// <remarks>Remember to destroy the Texture2D after use.</remarks>
		public static Texture2D CreateTexture2D(this RenderTexture texture) {
			Texture2D t2d = new Texture2D(texture.width, texture.height);
			RenderTexture.active = texture;
			t2d.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
			return t2d;
		}

		public static bool ReadValueAt(this RenderTexture texture, Vector2Int position, out Color value) {
			value = default;
			if(texture == null)
				return false;
			Rect bounds = new Rect(Vector2.zero, texture.Size());
			if(!bounds.Contains(position))
				return false;
			Texture2D t2d = new Texture2D(1, 1);
			RenderTexture.active = texture;
			t2d.ReadPixels(new Rect(position.x, position.y, 1, 1), 0, 0);
			value = t2d.GetPixel(0, 0);
			Object.Destroy(t2d);
			RenderTexture.active = null;
			return true;
		}

		/// <remarks>Remember to release the temporary RT after use.</remarks>
		public static RenderTexture Crop(this RenderTexture texture, RectInt rect) {
			if(texture == null || rect.width <= 0 || rect.height <= 0)
				return null;
			Material mat = GetPooledMaterial("NaniCore/Crop");
			mat.SetVector("_Size", (Vector2)texture.Size());
			mat.SetVector("_Range", new Vector4(rect.width, rect.height, rect.xMin, rect.yMin));
			var desc = texture.descriptor;
			desc.width = rect.width;
			desc.height = rect.height;
			var cropped = RenderTexture.GetTemporary(desc);
			Graphics.Blit(texture, cropped, mat);
			return cropped;
		}

		public static void InfectByValue(this RenderTexture texture, Color value, Vector2 radius) {
			var mat = GetPooledMaterial("NaniCore/InfectByValue");
			mat.SetVector("_Size", new Vector4(texture.width, texture.height, 0, 1));
			mat.SetColor("_Value", value);
			mat.SetVector("_Radius", new Vector4(radius.x, radius.y, 0, 1));
			texture.Apply(mat);
		}
		public static void InfectByValue(this RenderTexture texture, Color value, float radius)
			=> InfectByValue(texture, value, Vector2.one * radius);

		public static bool HasValue(this RenderTexture texture, Color value) {
			if(texture == null)
				return false;
			Vector2 size = texture.Size();
			float maxSize = Mathf.Max(size.x, size.y);
			int stepCount = Mathf.CeilToInt(Mathf.Sqrt(maxSize));
			Vector2 stepSize = size / stepCount;
			var copy = texture.Duplicate();
			for(int i = 0; i < stepCount; ++i)
				copy.InfectByValue(value, stepSize);
			copy.ReadValueAt(new Vector2Int(0, 0), out Color oneValue);
			copy.Destroy();
			float distance = Vector4.Distance(value, oneValue);
			return distance < 1f / 256;
		}

		private static bool ChunkTexture(RenderTexture texture, int chunkSize, out List<RectInt> chunks) {
			chunks = null;
			chunkSize = Mathf.Max(chunkSize, 1);
			if(texture.width <= chunkSize && texture.height <= chunkSize)
				return false;
			chunks = new List<RectInt>();
			int xC = texture.width > chunkSize ? 2 : 1;
			int yC = texture.height > chunkSize ? 2 : 1;
			var sizef = (Vector2)texture.Size();
			sizef.Scale(new Vector2(1f / xC, 1f / yC));
			var size = sizef.Ceil();
			for(int xI = 0; xI < xC; ++xI) {
				for(int yI = 0; yI < yC; ++yI) {
					var start = size;
					start.Scale(new Vector2Int(xI, yI));
					var end = start + size;
					end = Vector2Int.Min(end, texture.Size());
					var chunk = new RectInt(start, end - start);
					chunks.Add(chunk);
				}
			}
			return true;
		}

		private static bool FindAnyPositionOfValueNoDownSample(RenderTexture texture, Color value, int chunkSize, out Vector2Int position) {
			position = default;
			if(!texture.HasValue(value))
				return false;

			bool needsChunking = ChunkTexture(texture, chunkSize, out List<RectInt> chunks);
			if(needsChunking) {
				foreach(var chunk in chunks) {
					var cropped = texture.Crop(chunk);
					if(FindAnyPositionOfValueNoDownSample(cropped, value, chunkSize, out position)) {
						cropped.Destroy();
						position += chunk.position;
						return true;
					}
					cropped.Destroy();
				}
				return false;
			}
			else {
				position = ((Vector2)texture.Size() * .5f).Floor();
				return true;
			}
		}

		public static bool FindAnyPositionOfValue(this RenderTexture texture, Color value, float sampleRate, out Vector2Int position) {
			position = default;
			if(sampleRate > 1)
				sampleRate = 1;

			var downsampleSize = (Vector2)texture.Size() * sampleRate;
			var downsample = texture.Resample(new Vector2Int((int)downsampleSize.x, (int)downsampleSize.y));
			Vector2Int downsampledPosition;
			if(!FindAnyPositionOfValueNoDownSample(downsample, value, 1, out downsampledPosition)) {
				downsample.Destroy();
				return false;
			}

			// If optimized, can be more presice.

			position = (((Vector2)downsampledPosition + Vector2.one * .5f) / sampleRate).Floor();
			downsample.Destroy();
			return true;
		}

		/// <remarks>Remember to release the temporary RT after use.</remarks>
		public static RenderTexture Resample(this RenderTexture texture, Vector2Int size) {
			if(texture == null)
				return null;
			size = Vector2Int.Max(Vector2Int.one, size);
			var desc = texture.descriptor;
			desc.width = size.x;
			desc.height = size.y;
			var downsample = RenderTexture.GetTemporary(desc);
			Graphics.Blit(texture, downsample);
			return downsample;
		}

		public static void DrawCircle(this RenderTexture texture, Vector2Int offset, Color color, float radius) {
			var mat = GetPooledMaterial("NaniCore/DrawCircle");
			mat.SetVector("_Size", (Vector2)texture.Size());
			mat.SetVector("_Offset", (Vector2)offset);
			mat.SetColor("_Color", color);
			mat.SetFloat("_Radius", radius);
			texture.Apply(mat);
		}

		public static void Difference(this RenderTexture texture, RenderTexture difference) {
			var mat = GetPooledMaterial("NaniCore/Difference");
			mat.SetTexture("_DifferenceTex", difference);
			texture.Apply(mat);
		}

		public static void Intersect(this RenderTexture texture, RenderTexture difference) {
			var mat = GetPooledMaterial("NaniCore/Intersect");
			mat.SetTexture("_DifferenceTex", difference);
			texture.Apply(mat);
		}
	}
}