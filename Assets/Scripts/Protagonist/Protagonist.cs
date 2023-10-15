using UnityEngine;
using NaughtyAttributes;

namespace NaniCore.Loopool {
	public partial class Protagonist : MonoBehaviour {
		#region Singleton
		public static Protagonist instance;
		#endregion

		#region Serialized fields
		[Header("Default")]
		[SerializeField][Expandable] private ProtagonistProfile profile;
		#endregion

		#region Fields
		private bool profileDuplicated = false;
		private ProtagonistInputHandler inputHandler;
		#endregion

		#region Properties
		public ProtagonistProfile Profile {
			get {
				if(!Application.isPlaying)
					return profile;
				if(!profileDuplicated) {
					profile = Instantiate(profile);
					profileDuplicated = true;
				}
				return profile;
			}
		}
		#endregion

		#region Functions
		public void Cheat() {
			if(Raycast(out RaycastHit hitInfo)) {
				for(var target = hitInfo.transform; target != null; target = target.parent) {
					bool acted = false;
					foreach(var component in target.GetComponents<Component>()) {
						switch(component) {
							case AutomaticDoor door:
								door.ToggleOpeningState();
								acted = true;
								break;
							case PressurePlate plate:
								plate.Pressed = !plate.Pressed;
								acted = true;
								break;
							case OpticalLoopShape opticalLoopShape:
								opticalLoopShape.SendMessage("OnLoopShapeOpen", SendMessageOptions.DontRequireReceiver);
								break;
							case Interactable interactable:
								interactable.SendMessage("OnInteract", SendMessageOptions.DontRequireReceiver);
								acted = true;
								break;
						}
					}
					if(acted)
						break;
				}
			}
		}
		#endregion

		#region Life cycle
#if UNITY_EDITOR
		protected void OnValidate() {
			if(!Application.isPlaying) {
				ValidateControl();
			}
		}
#endif
		protected void Start() {
			if(GameManager.Instance == null) {
				string[] messages = {
					"There is no instance of GameManager in the scene!",
					"Please always make sure that there is one."
				};
				Debug.LogWarning(string.Join(" ", messages));
				Destroy(gameObject);
				return;
			}
			if(GameManager.Instance.Protagonist != null) {
				Destroy(gameObject);
				return;
			}
			GameManager.Instance.SendMessage("OnProtagonistCreated", this, SendMessageOptions.DontRequireReceiver);
			inputHandler = gameObject.EnsureComponent<ProtagonistInputHandler>();
			StartControl();
			StartInteraction();
		}

		protected void OnDestroy() {
			if(GameManager.Instance == null)
				return;
			if(GameManager.Instance.Protagonist != this)
				return;
			GameManager.Instance.SendMessage("OnProtagonistDestroyed", this, SendMessageOptions.DontRequireReceiver);
		}

		protected void FixedUpdate() {
			FixedUpdateControl();
		}

		protected void LateUpdate() {
			LateUpdateControl();
			LateUpdateInteraction();
		}
		#endregion
	}
}