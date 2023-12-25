using UnityEngine;
using NaughtyAttributes;

namespace NaniCore.Loopool {
	[ExecuteInEditMode]
	public partial class Protagonist : MonoBehaviour {
		#region Serialized fields
		[SerializeField][Expandable] private ProtagonistProfile profile;
		#endregion

		#region Fields
		private bool isProfileDuplicated = false;
		private ProtagonistInputHandler inputHandler;
		#endregion

		#region Properties
		public ProtagonistProfile Profile {
			get {
				if(!Application.isPlaying)
					return profile;
				if(!isProfileDuplicated) {
					profile = Instantiate(profile);
					isProfileDuplicated = true;
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
							case DtCarrier door:
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
		protected void Start() {
#if UNITY_EDITOR
			if(!Application.isPlaying) {
				return;
			}
#endif

			if(Profile == null) {
				Debug.LogWarning("No profile is configured for the protagonist.", this);
				return;
			}

			StartControl();
			StartInteraction();
			StartAudio();
		}

		protected void Update() {
#if UNITY_EDITOR
			if(!Application.isPlaying) {
				OnValidate();
				return;
			}
#endif
		}

#if UNITY_EDITOR
		protected void OnValidate() {
			ValidateControl();
		}
#endif

		protected void FixedUpdate() {
			FixedUpdateControl();
		}

		protected void LateUpdate() {
#if UNITY_EDITOR
			if(!Application.isPlaying) {
				return;
			}
#endif
			LateUpdateControl();
			LateUpdateInteraction();
		}
		#endregion
	}
}