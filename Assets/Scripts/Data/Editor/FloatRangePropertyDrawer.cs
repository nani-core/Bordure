using UnityEngine;
using UnityEditor;
using System;

namespace NaniCore {
	[CustomPropertyDrawer(typeof(FloatRange))]
	public class FloatRangePropertyDrawer : PropertyDrawer {
		FloatRange target;
		const float slitWidth = 4;
		const float labelWidth = 32;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			target = property.managedReferenceValue as FloatRange;
			if(target == null)
				property.managedReferenceValue = target = Activator.CreateInstance(typeof(FloatRange)) as FloatRange;
			return EditorGUIUtility.singleLineHeight;
		}


		private void DrawInputBox(ref Rect position, string label, ref float field) {
			EditorGUI.LabelField(new Rect(position) { width = labelWidth, }, label);
			field = EditorGUI.FloatField(new Rect(position) { xMin = position.xMin + labelWidth + slitWidth, }, field);
			position.x += position.width + slitWidth;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if(target == null)
				return;

			bool hasPivot = target is FloatPivotRange;
			float fieldCount = hasPivot ? 3 : 2;

			EditorGUI.LabelField(new Rect(position) { width = EditorGUIUtility.labelWidth }, label);

			Rect inputBoxPosition = position;
			inputBoxPosition.xMin += EditorGUIUtility.labelWidth;
			inputBoxPosition.width = (inputBoxPosition.width - slitWidth * (fieldCount - 1)) / fieldCount;

			if(hasPivot) {
				var pTarget = target as FloatPivotRange;
				var avatar = new FloatPivotRange(pTarget);
				EditorGUI.BeginChangeCheck();
				DrawInputBox(ref inputBoxPosition, "Min", ref avatar.min);
				DrawInputBox(ref inputBoxPosition, "Piv", ref avatar.pivot);
				DrawInputBox(ref inputBoxPosition, "Max", ref avatar.max);
				if(EditorGUI.EndChangeCheck()) {
					Undo.RecordObject(property.serializedObject.targetObject, $"Setting {fieldInfo.Name}");
					pTarget.min = avatar.min;
					pTarget.max = avatar.max;
					pTarget.pivot = avatar.pivot;
				}
			}
			else {
				var avatar = new FloatRange(target);
				EditorGUI.BeginChangeCheck();
				DrawInputBox(ref inputBoxPosition, "Min", ref avatar.min);
				DrawInputBox(ref inputBoxPosition, "Max", ref avatar.max);
				if(EditorGUI.EndChangeCheck()) {
					Undo.RecordObject(property.serializedObject.targetObject, $"Setting {fieldInfo.Name}");
					target.min = avatar.min;
					target.max = avatar.max;
				}
			}
		}
	}
}