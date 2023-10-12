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

			var targetMat = where.material;
			RenderTexture resultTexture;
			if(targetMat.mainTexture != null) {
				// This would be leaked on game end.
				resultTexture = targetMat.mainTexture.Duplicate();
			}
			else {
				resultTexture = RenderUtility.CreateScreenSizedRT();
				resultTexture.SetValue(Color.clear);
			}
			var mat = RenderUtility.GetPooledMaterial("NaniCore/ScreenUvReplace");
			mat.SetTexture("_OriginalTex", targetMat.mainTexture);
			mat.SetTexture("_ReplaceScreenTex", whatAppearance);
			mat.SetTexture("_ScreenUvTex", whereScreenUvTex);
			resultTexture.Apply(mat);
			if(targetMat.mainTexture is RenderTexture) {
				// Might be repeated stamping.
				(targetMat.mainTexture as RenderTexture).Destroy();
			}
			targetMat.mainTexture = resultTexture;

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