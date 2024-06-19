using UnityEngine;
using UnityEngine.UI;

namespace NaniCore.Bordure {
	public class AchievementsMenu : Menu {
		#region Serialized fields
		[SerializeField] private LayoutGroup layout;
		#endregion

		public override void OnEnter() {
			var am = GameManager.Instance.Achievement;

			foreach(var entry in am.Sheet.achievements) {
				bool isFinished = am.HasBeenFinished(entry.key);

				if(entry.hidden && !isFinished)
					continue;

				var obj = Instantiate(Resources.Load<GameObject>("Achievement Instance"), layout.transform);
				var instance = obj.GetComponent<AchievementInstance>();
				instance.Icon = entry.icon;
				instance.Title = entry.title;
				instance.Description = entry.description;
				instance.IsAchieved = isFinished;
			}

			layout.CalculateLayoutInputHorizontal();
			layout.CalculateLayoutInputVertical();
		}

		public override void OnExit() {
			layout.transform.DestroyAllChildren();
		}
	}
}
