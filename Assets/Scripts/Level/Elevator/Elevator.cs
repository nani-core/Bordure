using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public partial class Elevator : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private Transform anchor;

		[Header("Door")]
		[SerializeField] private DtCarrier door;
		[SerializeField][Min(0)] private float doorWaitTime;

		[Header("Buttons")]
		[SerializeField] private ElevatorButton buttonPrefab;
		[SerializeField] private Transform buttonAnchor;
		[SerializeField][Min(0)] private float buttonDistance = .25f;

		[Header("Levels")]
		[SerializeField] private Level currentLevel;
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
		protected void Start() {
			UpdateButtons();
		}
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
			StartCoroutine(SwitchLevelCoroutine(button.Level));
		}

		private IEnumerator SwitchLevelCoroutine(Level level) {
			door.IsOpened = false;
			yield return new WaitForSeconds(door.Duration);

			yield return new WaitForSeconds(doorWaitTime * .5f);
			SwitchLevel(level);
			yield return new WaitForSeconds(doorWaitTime * .5f);

			door.IsOpened = true;
			yield return new WaitForSeconds(door.Duration);
		}

		private void SwitchLevel(Level level) {
			if(currentLevel != null)
				currentLevel.gameObject.SetActive(false);
			currentLevel = GameManager.Instance.LoadLevel(level);

			var anchor = currentLevel.gameObject.GetComponentInChildren<ElevatorAnchor>();
			if(anchor == null) {
				Debug.LogWarning($"Level \"{currentLevel}\" has no elevator anchor!", currentLevel);
			}
			else {
				currentLevel.transform.AlignWith(anchor.transform, this.anchor);
			}
		}
		#endregion
	}
}
