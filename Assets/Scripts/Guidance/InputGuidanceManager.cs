using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace NaniCore.Bordure {
	public class InputGuidanceManager : MonoBehaviour {
		#region Serialized fields
		[SerializeField] private CanvasGroup canvasGroup;
		[SerializeField] private LayoutGroup instancesRoot;
		[SerializeField] private InputGuidanceSheet sheet;
		#endregion

		#region Fields
		private readonly Dictionary<string, InputGuidance> guidances = new();
		private readonly List<KeyValuePair<string, InputGuidanceInstance>> currentGuidances = new();
		#endregion

		#region Properties
		public IDictionary<string, InputGuidance> Guidances => guidances;
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

		private void A() {
			//InputAction act = default;
			//act.controls[0].device;
			//InputSystem.devices;
		}
		#endregion
	}
}
