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
		[SerializeField][Min(0)] protected float grabbingDistance;
		[SerializeField][Min(0)] protected float grabbingTime;
		#endregion

		#region Fields
		protected Interaction focusing;
		protected Grabbable grabbing;
		#endregion

		#region Life cycle
#if UNITY_EDITOR
		protected void ValidateInteraction() {
			EditorApplication.delayCall += () => FocusUi = focusUiMap.hovering;
		}
#endif

		protected void StartInteraction() {
			Focusing = null;
			FocusUi = focusUiMap.normal;
		}

		protected void UpdateInteraction() {
			if(Grabbing == null) {
				RaycastHit hitInfo;
				bool isHit = Physics.Raycast(camera.ViewportPointToRay(Vector2.one * .5f), out hitInfo, maxInteractionDistance, interactionLayerMask);
				Focusing = isHit ? hitInfo.collider.GetComponent<Interaction>() : null;
			}
		}
		#endregion

		#region Functions
		public float GrabbingDistance => grabbingDistance;
		public float GrabbingTime => grabbingTime;

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

		protected void UpdateFocusUi() {
			if(Grabbing)
				FocusUi = focusUiMap.grabbing;
			else if(Focusing)
				FocusUi = focusUiMap.hovering;
			else
				FocusUi = focusUiMap.normal;
		}

		public Interaction Focusing {
			get => focusing;
			set {
				if(focusing == value)
					return;

				if(focusing)
					focusing.SendMessage("OnFocusLeave");
				focusing = value;
				if(focusing)
					focusing.SendMessage("OnFocusEnter");

				UpdateFocusUi();
			}
		}

		public Grabbable Grabbing {
			get => grabbing;
			set {
				if(grabbing == value)
					return;

				if(grabbing)
					grabbing.SendMessage("OnGrabEnd");
				grabbing = value;
				if(grabbing)
					grabbing.SendMessage("OnGrabStart");

				UpdateFocusUi();
			}
		}

		public void Interact() {
			if(Grabbing == null)
				Focusing?.SendMessage("OnInteract");
			else
				Grabbing = null;
		}
		#endregion
	}
}