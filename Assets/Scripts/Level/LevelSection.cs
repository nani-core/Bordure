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
		private SphereCollider trigger = null;
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
		#endregion

		#region Interfaces
		public bool IsLoaded => gameObject.activeSelf;

		public void Load() {
			isLoaded = true;
			gameObject.SetActive(true);
			Debug.Log($"Level section \"{name}\" is loaded.", this);
		}
		#endregion

		#region Functions
		private void OnFirstEnabled() {
			if(trigger == null)
				GenerateTrigger();
			if(!isLoaded && !transform.GetLevel().IsLoaded)
				OnEnabledOnLoad();
		}

		private void GenerateTrigger() {
			trigger = gameObject.AddComponent<SphereCollider>();
			trigger.isTrigger = true;
			Bounds worldBounds = transform.CalculateBoundingBox();
			trigger.center = transform.worldToLocalMatrix.MultiplyPoint(worldBounds.center);
			trigger.radius = transform.worldToLocalMatrix.MultiplyVector(worldBounds.size).magnitude / Mathf.Sqrt(3.0f);
		}

		private void OnEnabledOnLoad() {
			gameObject.SetActive(false);
		}

		private void OnProtagonistEnter() {
			trigger.enabled = false;
			LoadConnectedSections();
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
