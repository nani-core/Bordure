using UnityEngine;
using NaughtyAttributes;

namespace NaniCore.Bordure {
	public partial class Protagonist : MonoBehaviour {
		#region Serialized fields
		[SerializeField][Expandable] private ProtagonistProfile profile;
		#endregion

		#region Fields
		private bool isProfileDuplicated = false;
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
			if(LookingAtObject != null) {
				for(var target = LookingAtObject.transform; target != null; target = target.parent) {
					bool acted = false;
					foreach(var component in target.GetComponents<Component>()) {
						switch(component) {
							case PressurePlate plate:
								plate.Pressed = !plate.Pressed;
								acted = true;
								break;
							case Loopshape loopshape:
								loopshape.Open();
								break;
						}
					}
					if(acted)
						break;
				}
			}
			GameManager.Instance.FinishAchievement("cheater");
		}
		#endregion

		#region Life cycle
		protected void Start() {
			if(Profile == null) {
				Debug.LogWarning("No profile is configured for the protagonist.", this);
				return;
			}
			InitializeControl();
		}

		protected void Update() {
			UpdateInteraction();
		}

		protected void FixedUpdate() {
			FixedUpdateControl(Time.fixedDeltaTime);
		}

		protected void OnEnable() {
			IsControlEnabled = true;
		}

		protected void OnDisable() {
			IsControlEnabled = false;
			isWalking = false;
			UpdateMovingAnimation();
		}
		#endregion
	}
}