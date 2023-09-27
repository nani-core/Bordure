using UnityEngine;
using NaughtyAttributes;

namespace NaniCore.Loopool {
	[RequireComponent(typeof(ProtagonistInputHandler))]
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

		protected void OnEnable() {
			instance = this;
		}

		protected void OnDisable() {
			instance = null;
		}

		protected void Start() {
			StartControl();
			StartInteraction();
		}

		protected void Update() {
			UpdateControl();
		}

		protected void LateUpdate() {
			LateUpdateInteraction();
		}
		#endregion
	}
}