namespace NaniCore.Bordure {
	/**
	 * For two tiers to be belonging to the same parent tier, they must share
	 * a common lower-bit sequence.
	 */
	public enum RigidbodyTier {
		Default = 0x0,

		Stone = 0x1,
		Ceramic = 0x1 | 0x2,

		Wood = 0x2,
		LightWood = 0x2 | 0x4,
		HeavyWood = 0x2 | 0x8,

		Plastic = 0x4,
		Rubber = 0x4 | 0x8,

		Metal = 0x8,
		Steel = 0x8 | 0x10,

		Dirt = 0x10,
		Mud = 0x10 | 0x20,
		Gravel = 0x10 | 0x30,
		Grass = 0x10 | 0x40,

		Carpet = 0x20,
		Rug = 0x20 | 0x40,

		Character = 0x40000000,
	}
}