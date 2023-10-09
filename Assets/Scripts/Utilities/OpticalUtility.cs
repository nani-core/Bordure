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

		public static void Stamp(Camera camera, Mrt what, MeshRenderer where) {
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
			mat.SetMatrix("_WorldToCam", camera.worldToCameraMatrix);
			mat.SetMatrix("_CameraProjection", camera.projectionMatrix);
			mat.SetMatrix("_WhereToWorld", where.transform.localToWorldMatrix);
			resultTexture.Apply(mat);
			if(targetMat.mainTexture is RenderTexture) {
				// Might be repeated stamping.
				(targetMat.mainTexture as RenderTexture).Destroy();
			}
			//Graphics.Blit(whatAppearance, resultTexture);
			targetMat.mainTexture = resultTexture;

			whatAppearance.Destroy();
		}

		public static void Stamp(Camera camera, Mrt what, GameObject where) {
			if(what == null || where == null)
				return;
			var whereArr = where.GetComponentsInChildren<MeshRenderer>();
			foreach(var whereRenderer in whereArr) {
				Stamp(camera, what, whereRenderer);
			}
		}
	}
}