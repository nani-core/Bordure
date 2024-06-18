using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public class AchievementManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private AchievementSheet sheet;
		[SerializeField] private Image icon;
		[SerializeField] private Text title;
		[SerializeField] private Text description;
		[SerializeField] private DtCarrier carrier;
		#endregion

		#region Fields
		private readonly HashSet<string> finishedAchievements = new();
		#endregion

		#region Life cycle
		protected void Start() {
			carrier.Progress = 0f;
		}
		#endregion

		#region Interfaces
		public void Finish(string key) {
			if(HasBeenFinished(key)) {
				Debug.LogWarning($"Warning: Achievement \"{key}\" has already been finished.");
				return;
			}

			StartCoroutine(FinishCoroutine(key));
		}

		public void ResetProgress() {
			finishedAchievements.Clear();
		}
		#endregion

		#region Functions
		private bool IsOpened {
			get => carrier.IsOpened;
			set => carrier.IsOpened = value;
		}

		private bool HasBeenFinished(string key) {
			return finishedAchievements.Contains(key);
		}

		private bool Find(string key, out AchievementEntry achievement) {
			foreach(var entry in sheet.achievements) {
				if(entry.key != key)
					continue;
				achievement = entry;
				return true;
			}
			achievement = default;
			return false;
		}

		private System.Collections.IEnumerator FinishCoroutine(string key) {
			if(!Find(key, out var achievement)) {
				Debug.LogWarning($"Warning: Cannot find achievement \"{key}\".");
				yield break;
			}

			icon.sprite = achievement.icon;
			title.text = achievement.title;
			description.text = achievement.description;

			IsOpened = true;

			Debug.Log($"Finished achievement \"{key}\".");

			yield return new WaitUntil(() => IsOpened);
			yield return new WaitForSeconds(2f);

			IsOpened = false;
		}
		#endregion
	}
}
