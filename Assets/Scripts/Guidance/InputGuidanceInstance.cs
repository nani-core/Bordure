using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace NaniCore.Bordure {
	public class InputGuidanceInstance : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private Image effectImage;
		[SerializeField] private LayoutGroup inputsRoot;
		#endregion

		#region Interfaces
		public Sprite EffectSprite {
			get => effectImage.sprite;
			set => effectImage.sprite = value;
		}

		public IEnumerable<Sprite> InputSprites {
			get {
				List<Sprite> results = new();
				foreach(Transform child in inputsRoot.transform.Children()) {
					if(!child.TryGetComponent(out Image image))
						continue;
					results.Add(image.sprite);
				}
				return results;
			}
			set {
				inputsRoot.transform.DestroyAllChildren();

				if(value == null)
					return;

				foreach(Sprite sprite in value) {
					if(sprite == null)
						continue;

					GameObject obj = Instantiate(Resources.Load<GameObject>("Interaction Guidance Input"), inputsRoot.transform);
					obj.name = $"Input ({sprite.name})";
					var image = obj.GetComponent<Image>();
					image.sprite = sprite;
				}

				inputsRoot.CalculateLayoutInputHorizontal();
				inputsRoot.CalculateLayoutInputVertical();
			}
		}

		public void Destroy() {
			Destroy(gameObject);
		}
		#endregion
	}
}
