using UnityEngine;

namespace NaniCore {
	public static class OpticalUtility {
		public static RenderTexture VisualizeUv(this MeshRenderer what, Camera camera) {
			if(what == null || camera == null)
				return null;

			var whatUv = RenderUtility.CreateScreenSizedRT();
			whatUv.SetValue(Color.clear);
			whatUv.RenderObject(what.gameObject, camera, RenderUtility.GetPooledMaterial("ManiCore/VisualizeUv"));
			return whatUv;
		}

		public static void Stamp(Camera camera, MeshRenderer what, MeshRenderer where) {
			if(camera == null)
				return;
			if(what == null)
				return;
			if(where == null)
				return;
			Debug.Log($"{what} => {where}", where);

			var whatAppearance = RenderUtility.CreateScreenSizedRT();
			whatAppearance.SetValue(Color.clear);
			whatAppearance.RenderObject(what.gameObject, camera, what.sharedMaterial);

			var targetMat = where.material;
			RenderTexture resultTexture;
			if(targetMat.mainTexture)
				resultTexture = targetMat.mainTexture.Duplicate();
			else {
				resultTexture = RenderUtility.CreateScreenSizedRT();
				resultTexture.SetValue(Color.clear);
			}
			var mat = RenderUtility.GetPooledMaterial("NaniCore/ScreenUvReplace");
			mat.SetTexture("_OriginalTex", targetMat.mainTexture);
			mat.SetTexture("_ReplaceScreenTex", whatAppearance);
			//camera.worldToCameraMatrix;
			mat.SetMatrix("_Projection", camera.transform.worldToLocalMatrix);
			mat.SetMatrix("_WhereToWorld", where.transform.localToWorldMatrix);
			mat.SetFloat("_ScreenRatio", (float)whatAppearance.width / whatAppearance.height);
			resultTexture.Apply(mat);
			if(targetMat.mainTexture is RenderTexture) {
				// Might be repeated stamping.
				(targetMat.mainTexture as RenderTexture).Destroy();
			}
			//Graphics.Blit(whatAppearance, resultTexture);
			targetMat.mainTexture = resultTexture;

			whatAppearance.Destroy();
		}

		public static void Stamp(Camera camera, GameObject what, GameObject where) {
			var whatArr = what.GetComponentsInChildren<MeshRenderer>();
			var whereArr = where.GetComponentsInChildren<MeshRenderer>();
			foreach(var whatRenderer in whatArr) {
				foreach(var whereRenderer in whereArr) {
					Stamp(camera, whatRenderer, whereRenderer);
				}
			}
		}
	}
}