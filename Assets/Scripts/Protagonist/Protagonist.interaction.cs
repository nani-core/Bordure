using UnityEngine;
using UnityEngine.UI;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NaniCore.UnityPlayground {
	public partial class Protagonist : MonoBehaviour {
		#region Serialized fields
		[Header("Interaction")]
		public new Camera camera;
		[SerializeField][Min(0)] protected float maxInteractionDistance;
		[SerializeField] protected LayerMask interactionLayerMask;
		[SerializeField] protected Image focusUi;
		[Serializable]
		public struct FocusUiMap {
			public Sprite normal;
			public Sprite hovering;
			public Sprite grabbing;
		}
		[SerializeField] protected FocusUiMap focusUiMap;
		#endregion

		#region Fields
		protected Interaction focusingInteraction;
		#endregion

		#region Life cycle
#if UNITY_EDITOR
		protected void ValidateInteraction() {
			EditorApplication.delayCall += () => FocusUi = focusUiMap.hovering;
		}
#endif

		protected void StartInteraction() {
			FocusingInteraction = null;
			FocusUi = focusUiMap.normal;
		}

		protected void UpdateInteraction() {
			RaycastHit hitInfo;
			bool isHit = Physics.Raycast(camera.ViewportPointToRay(Vector2.one * .5f), out hitInfo, maxInteractionDistance, interactionLayerMask);
			FocusingInteraction = isHit ? hitInfo.collider.GetComponent<Interaction>() : null;
		}
		#endregion

		#region Functions
		public Sprite FocusUi {
			get => focusUi?.sprite;
			set {
				if(focusUi != null) {
					focusUi.sprite = value;
					if(value == null)
						focusUi.rectTransform.sizeDelta = Vector2.zero;
					else
						focusUi.SetNativeSize();
				}
			}
		}

		public Interaction FocusingInteraction {
			get {
				// Don't optimize. Could avoid returning invalidated components.
				if(focusingInteraction == null)
					return null;
				return focusingInteraction;
			}
			set {
				if(focusingInteraction == value)
					return;

				focusingInteraction?.OnFocusLeave();
				focusingInteraction = value;
				focusingInteraction?.OnFocusEnter();

				FocusUi = focusingInteraction == null ? focusUiMap.normal : focusUiMap.hovering;
			}
		}
		#endregion
	}
}