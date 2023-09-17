using UnityEngine;
using UnityEditor;
using System;

namespace NaniCore.Loopool {
	[CustomPropertyDrawer(typeof(CheeseSlice))]
	public class CheeseSlicePropertyDrawer : PropertyDrawer {
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			object parent = property.serializedObject.targetObject;
			if(fieldInfo.GetValue(parent) == null)
				fieldInfo.SetValue(parent, Activator.CreateInstance(fieldInfo.FieldType));
			return base.GetPropertyHeight(property, label);
		}
	}
}