using UnityEngine;
using UnityEditor;

namespace NaniCore {
	[CustomPropertyDrawer(typeof(RangeAttribute))]
	public class RangeAttributePropertyDrawer : PropertyDrawer {
		FloatRange target;
		bool hasPivot;
		RangeAttribute range;

		const float slitWidth = 4;
		const float boxWidth = 52;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			target = fieldInfo.GetValue(property.serializedObject.targetObject) as FloatRange;
			hasPivot = target is FloatPivotRange;
			range = attribute as RangeAttribute;
			if(target == null)
				return EditorGUI.GetPropertyHeight(property, label, true);
			if(hasPivot)
				return EditorGUIUtility.singleLineHeight * 2f;
			return EditorGUIUtility.singleLineHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if(target == null) {
				EditorGUI.PropertyField(position, property, label, true);
				return;
			}

			position.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.LabelField(new Rect(position) { width = EditorGUIUtility.labelWidth }, label);
			position.xMin += EditorGUIUtility.labelWidth;

			FloatPivotRange pTarget = target as FloatPivotRange;
			float min = target.min, max = target.max, pivot = 0;
			if(hasPivot)
				pivot = pTarget.pivot;

			EditorGUI.BeginChangeCheck();

			min = EditorGUI.FloatField(new Rect(position) { xMax = position.xMin + boxWidth }, min);
			min = Mathf.Clamp(min, range.min, max);

			max = EditorGUI.FloatField(new Rect(position) { xMin = position.xMax - boxWidth }, max);
			max = Mathf.Clamp(max, min, range.max);

			float sliderXMin = position.xMin + boxWidth + slitWidth, sliderXMax = position.xMax - boxWidth - slitWidth;
			EditorGUI.MinMaxSlider(
				new Rect(position) {
					xMin = sliderXMin,
					xMax = sliderXMax,
				},
				ref min, ref max,
				range.min, range.max
			);

			if(hasPivot) {
				float
					xMin = Mathf.Lerp(sliderXMin, sliderXMax, Mathf.InverseLerp(range.min, range.max, min)),
					xMax = Mathf.Lerp(sliderXMin, sliderXMax, Mathf.InverseLerp(range.min, range.max, max));

				position.y += EditorGUIUtility.singleLineHeight;

				pivot = EditorGUI.FloatField(new Rect(position) {
					x = xMin - boxWidth - slitWidth,
					width = boxWidth,
				}, pivot);
				pivot = Mathf.Clamp(pivot, min, max);
				pivot = GUI.HorizontalSlider(
					new Rect(position) { xMin = xMin, xMax = xMax, },
					pivot, min, max
				);
			}

			if(EditorGUI.EndChangeCheck()) {
				Undo.RecordObject(property.serializedObject.targetObject, $"Setting {fieldInfo.Name}");
				target.min = min;
				target.max = max;
				if(hasPivot)
					pTarget.pivot = pivot;
			}
		}
	}
}