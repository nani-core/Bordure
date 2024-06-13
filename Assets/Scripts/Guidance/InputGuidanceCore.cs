using UnityEngine;

namespace NaniCore.Bordure {
	public enum InputGuidanceDevice {
		PC, Gamepad,
	}
		
	[System.Serializable]
	public struct InputGuidanceInput {
		public InputGuidanceDevice device;
		public Sprite[] inputSprites;
	}

	[System.Serializable]
	public struct InputGuidanceEntry {
		public string key;
		public Sprite effectSprite;
		public InputGuidanceInput[] inputs;
	}
}
