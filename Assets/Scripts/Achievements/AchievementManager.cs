using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public class AchievementManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private AchievementSheet sheet;
		[SerializeField] private LayoutGroup list;
		#endregion

		#region Fields
		private readonly HashSet<string> finishedAchievements = new();
		#endregion

		#region Interfaces
		public AchievementSheet Sheet => sheet;

		public void Finish(string key) {
			if(HasBeenFinished(key))
				return;

			finishedAchievements.Add(key);
			StartCoroutine(FinishCoroutine(key));
		}

		public void ResetProgress() {
			finishedAchievements.Clear();
		}

		public bool HasBeenFinished(string key) {
			return finishedAchievements.Contains(key);
		}
		#endregion

		#region Functions
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
			Find(key, out var achievement);

			GameObject obj = Instantiate(Resources.Load<GameObject>("Achievement Instance"), list.transform);
			list.CalculateLayoutInputHorizontal();
			list.CalculateLayoutInputVertical();

			var instance = obj.GetComponent<AchievementInstance>();
			instance.Icon = achievement.icon;
			instance.Title = achievement.title;
			instance.Description = achievement.description;

			instance.ShowPercent = 0f;
			yield return ProgressCoroutine(1f, p => instance.ShowPercent = p);
			yield return new WaitForSecondsRealtime(2f);
			yield return ProgressCoroutine(1f, p => instance.ShowPercent = 1 - p);

			Destroy(instance.gameObject);
			list.CalculateLayoutInputHorizontal();
			list.CalculateLayoutInputVertical();
		}

		private static System.Collections.IEnumerator ProgressCoroutine(float duration, System.Action<float> continuation) {
			for(float startTime = Time.unscaledTime, progress; (progress = (Time.unscaledTime - startTime) / duration) < 1.0f;) {
				continuation(progress);
				yield return new WaitForSecondsRealtime(Time.deltaTime);
			}
			continuation(1.0f);
		}
		#endregion
	}
}