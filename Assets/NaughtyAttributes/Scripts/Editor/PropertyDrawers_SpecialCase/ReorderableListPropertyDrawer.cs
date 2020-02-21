using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace NaughtyAttributes.Editor
{
	public class ReorderableListPropertyDrawer : SpecialCasePropertyDrawerBase
	{
		public static readonly ReorderableListPropertyDrawer Instance = new ReorderableListPropertyDrawer();

		private readonly Dictionary<string, ReorderableList> _reorderableListsByPropertyName = new Dictionary<string, ReorderableList>();

		private string GetPropertyKeyName(SerializedProperty property)
		{
			return property.serializedObject.targetObject.GetInstanceID() + "/" + property.name;
		}

		protected override void OnGUI_Internal(SerializedProperty property, GUIContent label)
		{
			if (!property.isArray)
			{
				string message = nameof(ReorderableListAttribute) + " can be used only on arrays or lists";
				NaughtyEditorGUI.HelpBox_Layout(message, MessageType.Warning, context: property.serializedObject.targetObject);
				EditorGUILayout.PropertyField(property, true);
				return;
			}

			string key = GetPropertyKeyName(property);

			if (!_reorderableListsByPropertyName.ContainsKey(key))
			{
				var reorderableList = new ReorderableList(property.serializedObject, property, true, true, true, true)
				{
					drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, $"{label.text}: {property.arraySize}", EditorStyles.boldLabel); },

					drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
					{
						var elementProperty = property.GetArrayElementAtIndex(index);
						rect.y += 1.0f;
						rect.x += 10.0f;
						rect.width -= 10.0f;

						EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, 0.0f), elementProperty, true);
					},

					elementHeightCallback = index => EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(index)) + 4.0f
				};

				_reorderableListsByPropertyName[key] = reorderableList;
			}

			_reorderableListsByPropertyName[key].DoLayoutList();
		}

		public void ClearCache()
		{
			_reorderableListsByPropertyName.Clear();
		}
	}
}