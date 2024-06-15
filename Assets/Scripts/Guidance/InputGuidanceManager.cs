using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

namespace NaniCore.Bordure {
	public class InputGuidanceManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private CanvasGroup canvasGroup;
		[SerializeField] private LayoutGroup instancesRoot;
		[SerializeField] private InputGuidanceSheet sheet;
		#endregion

		#region Fields
		private readonly Dictionary<string, InputGuidanceEntry> guidances = new();
		private readonly List<KeyValuePair<string, InputGuidanceInstance>> currentGuidances = new();
		private readonly Dictionary<InputGuidanceDevice, bool> controlSchemeValidation = new();
		#endregion

		#region Properties
		public IDictionary<string, InputGuidanceEntry> Guidances => guidances;
		#endregion

		#region Interfaces

		public void ShowByKey(string key) {
			if(GetGuidanceObjByKey(key) != null) {
				Debug.LogWarning($"Warning: Input guidance of key \"{key}\" is already shown.");
				return;
			}

			GameObject obj = Instantiate(Resources.Load<GameObject>("Interaction Guidance Instance"), instancesRoot.transform);
			InputGuidanceInstance instance = obj.GetComponent<InputGuidanceInstance>();
			currentGuidances.Add(new(key, instance));

			// TODO: Filter its input sprites.
			var guidance = guidances[key];
			instance.EffectSprite = guidance.effectSprite;
			List<Sprite> inputSprites = new();
			foreach(var input in guidance.inputs) {
				if(!IsControlSchemeValidated(input.device))
					continue;
				inputSprites.AddRange(input.inputSprites);
			}
			instance.InputSprites = inputSprites;

			instancesRoot.CalculateLayoutInputHorizontal();
			instancesRoot.CalculateLayoutInputVertical();
		}

		public void HideByKey(string key) {
			InputGuidanceInstance instance = GetGuidanceObjByKey(key);
			if(instance == null) {
				Debug.LogWarning($"Warning: Input guidance of key \"{key}\" is already hidden.");
				return;
			}

			// TODO: Ease it.
			currentGuidances.RemoveAll(pair => pair.Key == key);
			instance.Destroy();
		}

		public void UpdateControlSchemeValidations() {
			if(!GameManager.Instance.UsesProtagonist) {
				controlSchemeValidation.Clear();
				return;
			}

			var schemes = GameManager.Instance.Protagonist.GetComponent<PlayerInput>().actions.controlSchemes;

			foreach(var scheme in schemes) {
				InputGuidanceDevice key = default;
				switch(scheme.name) {
					case "PC":
						key = InputGuidanceDevice.PC;
						break;
					case "Gamepad":
						key = InputGuidanceDevice.Gamepad;
						break;
					default:
						continue;
				}
				controlSchemeValidation[key] = ValidateControlScheme(scheme);
			}
		}
		#endregion

		#region Life cycle
		protected void Start() {
			InitializeGuidanceMap();
		}
		#endregion

		#region Functions
		private void InitializeGuidanceMap() {
			if(sheet == null) {
				Debug.LogError("Error: No input guidance sheet configured.", this);
				return;
			}

			bool duplicationFlag = false;
			foreach(var guidance in sheet.guidances) {
				string key = guidance.key;
				if(guidances.ContainsKey(key)) {
					Debug.LogWarning($"Warning: Input guidance key \"{key}\" is duplicated.", this);
					duplicationFlag |= true;
					continue;
				}
				guidances.Add(key, guidance);
			}
			if(duplicationFlag) {
				Debug.LogError("Error: There are duplications of interaction key.", this);
				return;
			}
		}

		private InputGuidanceInstance GetGuidanceObjByKey(string key) {
			foreach(var (k, instance) in currentGuidances) {
				if(k == key)
					return instance;
			}
			return null;
		}

		private bool ValidateControlScheme(in InputControlScheme scheme) {
			foreach(var device in InputSystem.devices) {
				if(scheme.SupportsDevice(device))
					return true;
			}
			return false;
		}

		private bool IsControlSchemeValidated(InputGuidanceDevice device) {
			return controlSchemeValidation.ContainsKey(device) && controlSchemeValidation[device];
		}
		#endregion
	}
}
