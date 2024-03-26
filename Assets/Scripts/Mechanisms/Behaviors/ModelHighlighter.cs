using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;

namespace NaniCore.Bordure {
	public class ModelHighlighter : MonoBehaviour {
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
		public void Highlight() => Work(HighlightCoroutine());

		public void LightOn() => Work(LightCoroutine());

		public void LightOff() {
			IsWorking = false;
		}
		#endregion

		#region Fields
		private Coroutine coroutine = null;
		private bool isWorking = false;

		private float totalEmmisiveIntensity = 0.0f;
		private Material material = null;
		private readonly HashSet<MeshRenderer> renderers = new();
		#endregion

		#region Functions
		#region Properties
		private float Alpha {
			get => material.GetColor("_UnlitColor").a;
			set {
				var color = Rgb;
				color.a = value;
				material.SetColor("_UnlitColor", color);
			}
		}

		private float Strength {
			get => material.GetFloat("_EmissiveIntensity") / totalEmmisiveIntensity;
			set {
				material.SetFloat("_EmissiveIntensity", value * totalEmmisiveIntensity);
				Alpha = value;
			}
		}

		private float TotalEmmisiveIntensity {
			get => totalEmmisiveIntensity;
			set {
				totalEmmisiveIntensity = value;
				Strength = Strength;
			}
		}

		private Color Rgb {
			get => material.GetColor("_UnlitColor");
			set {
				var color = Rgb;
				color.r = value.r;
				color.g = value.g;
				color.b = value.b;
				material.SetColor("_UnlitColor", color);
				color.a = 1.0f;
				material.SetColor("_EmmisiveColor", color);
			}
		}
		#endregion

		#region Working
		private bool IsWorking {
			get => isWorking;
			set {
				if(isWorking == value)
					return;
				if(isWorking = value)
					InitializeResources();
				else
					FinalizeResources();
			}
		}

		private void Work(IEnumerator enumerator) => StartCoroutine(WorkCoroutine(enumerator));

		private IEnumerator WorkCoroutine(IEnumerator enumerator) {
			if(IsWorking)
				IsWorking = false;
			IsWorking = true;
			yield return coroutine = StartCoroutine(enumerator);
			IsWorking = false;
		}

		private void InitializeResources() {
			// Initialize working material.
			material = new Material(GameManager.Instance.Settings.highlightMaterial);
			Rgb = GameManager.Instance.Settings.highlightColor;

			foreach(var referenceRender in GetComponentsInChildren<MeshRenderer>()) {
				if(!referenceRender.TryGetComponent<MeshFilter>(out var referenceFilter))
					continue;
				
				// Create the phantom game object.
				var phantom = new GameObject($"Highlight Phantom ({referenceRender.gameObject.name})");
				phantom.transform.SetParent(referenceRender.transform, false);
				phantom.AddComponent<MeshFilter>().sharedMesh = referenceFilter.sharedMesh;
				var renderer = phantom.AddComponent<MeshRenderer>();
				renderers.Add(renderer);

				// Apply the material.
				// There might be multiple submeshes, so we assign by array here.
				int subMeshCount = referenceFilter.sharedMesh.subMeshCount;
				var matArr = new Material[subMeshCount];
				for(int i = 0; i < subMeshCount; ++i) {
					Material chosenMat = material;
					if(selectiveSubmesh) {
						if(submeshIndices.Contains(i))
							chosenMat = GameManager.Instance.Settings.highlightOffMaterial;
					}
					matArr[i] = chosenMat;
				}
				renderer.sharedMaterials = matArr;
			}
		}

		private void FinalizeResources() {
			if(coroutine != null) {
				StopCoroutine(coroutine);
				coroutine = null;
			}
			if(material != null) {
				Destroy(material);
				material = null;
			}
			if(renderers.Count > 0) {
				foreach(var renderer in renderers) {
					if(renderer == null)
						continue;
					Destroy(renderer.gameObject);
				}
				renderers.Clear();
			}
		}
		#endregion

		private IEnumerator HighlightCoroutine() {
			TotalEmmisiveIntensity = GameManager.Instance.Settings.highlightEmmisiveIntensity;

			float duration = GameManager.Instance.Settings.highlightDuration;
			for(float startTime = Time.time, now; (now = Time.time) - startTime < duration;) {
				float t = (now - startTime) / duration;
				Strength = Mathf.Exp(10 * Mathf.Log(1 - t));
				yield return new WaitForEndOfFrame();
			}
		}

		private IEnumerator LightCoroutine() {
			TotalEmmisiveIntensity = GameManager.Instance.Settings.lightEmmisiveIntensity;
			Strength = .5f;
			yield return new WaitWhile(() => true);
		}
		#endregion

		#region Life cycle
		protected void OnDestroy() {
			FinalizeResources();
		}
		#endregion
	}
}
