using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

namespace NaniCore.Stencil {
	public class Elevator : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private Transform doorTransform;
		[SerializeField] private ElevatorButton buttonPrefab;
		[SerializeField] private Transform buttonAnchor;
		[SerializeField][Min(0)] private float buttonDistance = .25f;
		[SerializeField] private Level level;
		[SerializeField] private List<Level> levels = new();
		#endregion

		#region Interfaces
		public List<Level> Levels {
			get => levels;
			set {
				levels = value;
				UpdateButtons();
			}
		}
		#endregion

		#region Life cycle
#if UNITY_EDITOR
		protected void OnValidate() {
			EditorApplication.delayCall += UpdateButtons;
		}
#endif
		#endregion

		#region Functions
		private void UpdateButtons() {
			buttonAnchor.DestroyAllChildren();
			for(int i = 0; i < levels.Count; ++i) {
				var button = buttonPrefab.gameObject.InstantiatePrefab(buttonAnchor).GetComponent<ElevatorButton>();
				button.transform.localPosition = Vector3.up * (buttonDistance * i);
				button.Level = levels[i];
				button.OnClick.AddListener(() => OnButtonClicked(button));
			}
		}

		private void OnButtonClicked(ElevatorButton button) {
			level.gameObject.SetActive(false);
			level = GameManager.Instance.LoadLevel(button.Level);

			var anchor = level.gameObject.GetComponentInChildren<ElevatorAnchor>();
			if(anchor == null) {
				Debug.LogWarning($"Level \"{level}\" has no elevator anchor!", level);
			}
			else {
				level.transform.AlignWith(anchor.transform, doorTransform);
			}
		}
		#endregion
	}
}
