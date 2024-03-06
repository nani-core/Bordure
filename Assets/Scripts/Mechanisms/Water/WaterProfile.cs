using UnityEngine;

namespace NaniCore.Stencil {
	[CreateAssetMenu(menuName = "Nani Core/Water Profile")]
	public class WaterProfile : ScriptableObject {
		[SerializeField][Min(0)] public float density;
		[SerializeField][Min(0)] public float damp;
	}
}
