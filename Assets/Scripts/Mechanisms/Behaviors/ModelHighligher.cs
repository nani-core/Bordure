using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;

namespace NaniCore.Stencil {
	public class ModelHighligher : MonoBehaviour {
		#region Serialized fields
		public Color color = Color.white;
		[Min(0)] public float duration = 1.0f;
		[Label("Material")] public Material templateMaterial = null;
		#endregion

		#region Interfaces
		public void Highlight() {
			if(templateMaterial == null) {
				Debug.LogWarning($"{this} has no material set.", this);
				return;
			}

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
			workingMaterial = new Material(templateMaterial);
			workingMaterial.SetColor("_UnlitColor", color);
			workingMaterial.SetColor("_EmmisiveColor", color);

			foreach(var referenceRender in GetComponentsInChildren<MeshRenderer>()) {
				if(!referenceRender.TryGetComponent<MeshFilter>(out var referenceFilter))
					continue;
				
				// Create the phantom game object.
				var phantom = new GameObject($"Highlight Phantom ({referenceRender.gameObject.name})");
				phantom.transform.SetParent(referenceRender.transform, false);
				phantom.AddComponent<MeshFilter>().sharedMesh = referenceFilter.sharedMesh;
				var renderer = phantom.AddComponent<MeshRenderer>();
				renderer.sharedMaterial = workingMaterial;
				workingRenderers.Add(renderer);
			}
		}

		private IEnumerator HighlightCoroutine() {
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

			float fullIntensity = templateMaterial.GetFloat("_EmissiveIntensity");
			float _intensity = fullIntensity * Mathf.Exp(10 * Mathf.Log(a));
			workingMaterial.SetFloat("_EmissiveIntensity", _intensity);

			Color _color = color;
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
