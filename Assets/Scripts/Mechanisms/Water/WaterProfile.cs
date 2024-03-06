using UnityEngine;

namespace NaniCore.Stencil {
	[CreateAssetMenu(menuName = "Nani Core/Water Profile")]
	public class WaterProfile : ScriptableObject {
		[SerializeField][Min(0)] public float density;
		[SerializeField][Range(0, 1)] public float damp;
	}
}
