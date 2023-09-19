using UnityEngine;
using UnityEditor;

namespace NaniCore.Loopool {
	[CustomEditor(typeof(PositionalLoopShape))]
	public class PositionalLoopShapeEditor : Editor {
		private PositionalLoopShape Target => target as PositionalLoopShape;

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			EditorGUILayout.Space();
			if(GUILayout.Button("Align Scene Camera to View")) {
				foreach(SceneView sceneView in SceneView.sceneViews) {
					sceneView.AlignViewToObject(Target.origin);
				}
			}
		}
	}
}