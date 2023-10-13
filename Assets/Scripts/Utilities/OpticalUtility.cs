using UnityEngine;

namespace NaniCore.Loopool {
	public static class OpticalUtility {
		public static RenderTexture VisualizeUv(this MeshRenderer what, Camera camera) {
			if(what == null || camera == null)
				return null;

			var whatUv = RenderUtility.CreateScreenSizedRT();
			whatUv.SetValue(Color.clear);
			whatUv.RenderObject(what.gameObject, camera, RenderUtility.GetPooledMaterial("NaniCore/VisualizeUv"));
			return whatUv;
		}

		public static void Stamp(Camera camera, Mrt what, MeshRenderer where, RenderTexture whereScreenUvTex) {
			if(camera == null || what == null || where == null)
				return;
			Debug.Log($"Stamping {what} on {where} under {camera}", where);

			var whatAppearance = what.MaskedTexture.Duplicate();

			var handler = where.gameObject.EnsureComponent<StampHandler>();
			handler.Initialize();

			RenderTexture stampingTexture = RenderUtility.CreateScreenSizedRT();
			stampingTexture.SetValue(Color.clear);
			var mat = RenderUtility.GetPooledMaterial("NaniCore/ScreenUvReplace");
			mat.SetTexture("_OriginalTex", handler.Material.mainTexture);
			mat.SetTexture("_ReplaceScreenTex", whatAppearance);
			mat.SetTexture("_ScreenUvTex", whereScreenUvTex);
			stampingTexture.Apply(mat);

			handler.SetStampingTexture(stampingTexture);

			whatAppearance.Destroy();
		}

		public static void Stamp(Camera camera, Mrt what, GameObject where) {
			if(camera == null)
				return;
			if(what == null || where == null)
				return;
			var whereScreenUvTex = RenderUtility.CreateScreenSizedRT(RenderTextureFormat.ARGBFloat);
			Material meshToScreenMat = RenderUtility.GetPooledMaterial("NaniCore/MeshUvToScreenUv");
			meshToScreenMat.SetFloat("_Fov", camera.fieldOfView);
			whereScreenUvTex.RenderObject(where.gameObject, camera, meshToScreenMat);
			var whereArr = where.GetComponentsInChildren<MeshRenderer>();
			foreach(var whereRenderer in whereArr) {
				Stamp(camera, what, whereRenderer, whereScreenUvTex);
			}
			whereScreenUvTex.Destroy();
		}
	}
}