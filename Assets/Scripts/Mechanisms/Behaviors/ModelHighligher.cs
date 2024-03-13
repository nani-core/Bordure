using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;

namespace NaniCore.Stencil {
	public class ModelHighligher : MonoBehaviour {
		#region Serialized fields
		/// <summary>
		/// Do we want to only highlight certain submeshes or not.
		/// </summary>
		public bool selectiveSubmesh = false;
		/// <summary>
		/// The indices of the submeshes to highlight.
		/// Would not take effect if `selectiveSubmesh` is not set.
		/// </summary>
		[ShowIf("selectiveSubmesh")] public List<int> submeshIndices = new();
		#endregion

		#region Interfaces
		public void Highlight() {
			InitializeHighlight();
			workingCoroutine = StartCoroutine(HighlightCoroutine());
		}
		#endregion

		#region Fields
		private Coroutine workingCoroutine = null;
		private Material workingMaterial = null;
		private readonly HashSet<MeshRenderer> workingRenderers = new();
		#endregion

		#region Functions
		private void InitializeHighlight() {
			// Initialize working material.
			workingMaterial = new Material(GameManager.Instance.Settings.defaultHighlightMaterial);
			workingMaterial.SetColor("_UnlitColor", GameManager.Instance.Settings.defaultHighlightColor);
			workingMaterial.SetColor("_EmmisiveColor", GameManager.Instance.Settings.defaultHighlightColor);

			foreach(var referenceRender in GetComponentsInChildren<MeshRenderer>()) {
				if(!referenceRender.TryGetComponent<MeshFilter>(out var referenceFilter))
					continue;
				
				// Create the phantom game object.
				var phantom = new GameObject($"Highlight Phantom ({referenceRender.gameObject.name})");
				phantom.transform.SetParent(referenceRender.transform, false);
				phantom.AddComponent<MeshFilter>().sharedMesh = referenceFilter.sharedMesh;
				var renderer = phantom.AddComponent<MeshRenderer>();
				workingRenderers.Add(renderer);

				// Apply the material.
				// There might be multiple submeshes, so we assign by array here.
				int subMeshCount = referenceFilter.sharedMesh.subMeshCount;
				var matArr = new Material[subMeshCount];
				for(int i = 0; i < subMeshCount; ++i) {
					Material chosenMat = workingMaterial;
					if(selectiveSubmesh) {
						if(submeshIndices.Contains(i))
							chosenMat = GameManager.Instance.Settings.highlightOffMaterial;
					}
					matArr[i] = chosenMat;
				}
				renderer.sharedMaterials = matArr;
			}
		}

		private IEnumerator HighlightCoroutine() {
			float duration = GameManager.Instance.Settings.defaultHighlightDuration;
			for(float startTime = Time.time, now; (now = Time.time) - startTime < duration;) {
				float t = (now - startTime) / duration;
				SetProgress(t);
				yield return new WaitForEndOfFrame();
			}
			FinalizeHighlight();
		}

		private void FinalizeHighlight() {
			if(workingCoroutine != null) {
				StopCoroutine(workingCoroutine);
				workingCoroutine = null;
			}
			if(workingMaterial != null) {
				Destroy(workingMaterial);
				workingMaterial = null;
			}
			if(workingRenderers.Count > 0) {
				foreach(var renderer in workingRenderers)
					Destroy(renderer.gameObject);
				workingRenderers.Clear();
			}
		}

		private void SetProgress(float t) {
			float a = 1 - t;

			float fullIntensity = GameManager.Instance.Settings.defaultHighlightMaterial.GetFloat("_EmissiveIntensity");
			float _intensity = fullIntensity * Mathf.Exp(10 * Mathf.Log(a));
			workingMaterial.SetFloat("_EmissiveIntensity", _intensity);

			Color _color = GameManager.Instance.Settings.defaultHighlightColor;
			_color.a = a;
			workingMaterial.SetColor("_UnlitColor", _color);
		}
		#endregion

		#region Life cycle
		protected void OnDestroy() {
			FinalizeHighlight();
		}
		#endregion
	}
}
