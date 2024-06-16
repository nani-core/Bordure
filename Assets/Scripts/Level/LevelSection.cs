using UnityEngine;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public class LevelSection : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private List<LevelSection> connectedSections = new();
		#endregion

		#region Fields
		private bool hasEnabled = false;
		private bool isLoaded = false;
		private Collider trigger = null;
		#endregion

		#region Life cycle
		protected void OnEnable() {
			if(!hasEnabled) {
				OnFirstEnabled();
				hasEnabled = true;
			}
		}

		protected void OnTriggerEnter(Collider other) {
			if(other.transform != GameManager.Instance.Protagonist?.transform)
				return;

			OnProtagonistEnter();
		}

		protected void OnTriggerExit(Collider other) {
			if(other.transform != GameManager.Instance.Protagonist?.transform)
				return;

			OnProtagonistExit();
		}

		private void OnFirstEnabled() {
			if(trigger == null)
				GenerateTrigger();
			if(!isLoaded && !transform.GetLevel().IsLoaded)
				OnEnabledOnLoad();
		}

		private void OnEnabledOnLoad() {
			gameObject.SetActive(false);
		}

		private void OnProtagonistEnter() {
			LoadConnectedSections();
			UseReflectionProbes = true;
		}

		private void OnProtagonistExit() {
			UseReflectionProbes = false;
		}
		#endregion

		#region Interfaces
		public bool IsLoaded => gameObject.activeSelf;

		[ContextMenu("Load")]
		public void Load() {
			isLoaded = true;
			if(gameObject.activeInHierarchy)
				return;
			gameObject.SetActive(true);
			Debug.Log($"Level section \"{name}\" is loaded.", this);
		}
		#endregion

		#region Functions
		private bool UseReflectionProbes {
			set {
				foreach(var probe in GetComponentsInChildren<ReflectionProbe>(true)) {
					probe.enabled = value;
				}
			}
		}

		private void GenerateTrigger() {
			var sphere = gameObject.AddComponent<SphereCollider>();
			Bounds worldBounds = transform.CalculateBoundingBox();
			sphere.center = transform.worldToLocalMatrix.MultiplyPoint(worldBounds.center);
			sphere.radius = transform.worldToLocalMatrix.MultiplyVector(worldBounds.size).magnitude / Mathf.Sqrt(3.0f);

			trigger = sphere;
			trigger.isTrigger = true;
			trigger.gameObject.layer = LayerMask.NameToLayer("LevelSection");
		}

		private void LoadConnectedSections() {
			LoadConnectedSections(new() { this });
		}

		private void LoadConnectedSections(List<LevelSection> loadedSections) {
			foreach(var section in connectedSections) {
				if(loadedSections.Contains(section) || section.IsLoaded)
					continue;
				section.Load();
			}
		}
		#endregion
	}
}
