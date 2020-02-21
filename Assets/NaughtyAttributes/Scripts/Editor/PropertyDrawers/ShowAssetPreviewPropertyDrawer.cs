using UnityEngine;
using UnityEditor;

namespace NaughtyAttributes.Editor
{
	[CustomPropertyDrawer(typeof(ShowAssetPreviewAttribute))]
	public class ShowAssetPreviewPropertyDrawer : PropertyDrawerBase
	{
		protected override float GetPropertyHeight_Internal(SerializedProperty property, GUIContent label)
		{
			if (property.propertyType == SerializedPropertyType.ObjectReference)
			{
				Texture2D previewTexture = GetAssetPreview(property);
				if (previewTexture != null)
				{
					var previewAttribute = PropertyUtility.GetAttribute<ShowAssetPreviewAttribute>(property);
					return GetPropertyHeight(property) + GetAssetPreviewSize(property, previewAttribute).y;
				}
				else
				{
					return GetPropertyHeight(property);
				}
			}
			else
			{
				return GetPropertyHeight(property) + GetHelpBoxHeight();
			}
		}

		protected override void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(rect, label, property);

			if (property.propertyType == SerializedPropertyType.ObjectReference)
			{
				Rect propertyRect = new Rect()
				{
					x = rect.x,
					y = rect.y,
					width = rect.width,
					height = EditorGUIUtility.singleLineHeight
				};

				EditorGUI.PropertyField(propertyRect, property, label);

				Texture2D previewTexture = GetAssetPreview(property);
				if (previewTexture != null)
				{
					var previewAttribute = PropertyUtility.GetAttribute<ShowAssetPreviewAttribute>(property);
					var previewSize = GetAssetPreviewSize(property, previewAttribute);
					var previewPosition = rect.x + NaughtyEditorGUI.GetIndentLength(rect);
					switch (previewAttribute.Alignment)
					{
						case TextAlignment.Center:
							previewPosition += (rect.width - previewSize.x) / 2;
							break;
						case TextAlignment.Right:
							previewPosition = rect.max.x - previewSize.x;
							break;
					}
					Rect previewRect = new Rect
					{
						x = previewPosition,
						y = rect.y + EditorGUIUtility.singleLineHeight,
						width = rect.width,
						height = previewSize.y
					};

					GUI.Label(previewRect, previewTexture);
				}
			}
			else
			{
				string message = property.name + " doesn't have an asset preview";
				DrawDefaultPropertyAndHelpBox(rect, property, message, MessageType.Warning);
			}

			EditorGUI.EndProperty();
		}

		private Texture2D GetAssetPreview(SerializedProperty property)
		{
			if (property.propertyType == SerializedPropertyType.ObjectReference)
			{
				if (property.objectReferenceValue != null)
				{
					Texture2D previewTexture = AssetPreview.GetAssetPreview(property.objectReferenceValue);
					return previewTexture;
				}

				return null;
			}

			return null;
		}

		private Vector2 GetAssetPreviewSize(SerializedProperty property, ShowAssetPreviewAttribute assetPreviewAttribute)
		{
			Texture2D previewTexture = GetAssetPreview(property);
			if (previewTexture == null)
			{
				return Vector2.zero;
			}
			else
			{
				int width = Mathf.Clamp(assetPreviewAttribute.Width, 0, previewTexture.width);
				int height = Mathf.Clamp(assetPreviewAttribute.Height, 0, previewTexture.height);

				return new Vector2(width, height);
			}
		}
	}
}
