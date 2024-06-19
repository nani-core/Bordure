using UnityEngine;
using UnityEngine.UI;

namespace NaniCore.Bordure {
	public class AchievementInstance : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private RectMask2D mask;
		[SerializeField] private RectTransform root;
		[SerializeField] private Image icon;
		[SerializeField] private Text title;
		[SerializeField] private Text description;
		#endregion

		#region Interfaces
		public float ShowPercent {
			set {
				value = Mathf.Clamp01(value);

				var pos = root.anchoredPosition;
				pos.y = root.rect.height * (value - 1);
				root.anchoredPosition = pos;
			}
		}

		public Sprite Icon {
			get => icon.sprite;
			set => icon.sprite = value;
		}

		public string Title {
			get => title.text;
			set => title.text = value;
		}

		public string Description {
			get => description.text;
			set => description.text = value;
		}
		#endregion
	}
}
