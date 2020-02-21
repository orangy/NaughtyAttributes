using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NaughtyAttributes.Editor
{
	public static class PropertyUtility
	{
		private static MethodInfo _getFieldInfoAndStaticTypeFromProperty;

		static PropertyUtility()
		{
			var assembly = typeof(SceneView).Assembly;
			var utility = assembly.GetType("UnityEditor.ScriptAttributeUtility");
			if (utility != null)
			{
				_getFieldInfoAndStaticTypeFromProperty = utility.GetMethod("GetFieldInfoAndStaticTypeFromProperty", BindingFlags.NonPublic | BindingFlags.Static);
			}
		}

		private static FieldInfo GetFieldInfoAndStaticTypeFromProperty(SerializedProperty property)
		{
			if (_getFieldInfoAndStaticTypeFromProperty != null)
			{
				Type type = null;
				var result = _getFieldInfoAndStaticTypeFromProperty.Invoke(null, new object[] {property, type});
				return result as FieldInfo;
			}

			return null;
		}

		public static T GetAttribute<T>(SerializedProperty property) where T : class
		{
			T[] attributes = GetAttributes<T>(property);
			return (attributes.Length > 0) ? attributes[0] : null;
		}

		public static T[] GetAttributes<T>(SerializedProperty property) where T : class
		{
			var fieldInfo = GetFieldInfoAndStaticTypeFromProperty(property);
			if (fieldInfo == null)
				return Array.Empty<T>();

			return (T[]) fieldInfo.GetCustomAttributes(typeof(T), true);
		}

		public static string GetLabel(SerializedProperty property)
		{
			LabelAttribute labelAttribute = GetAttribute<LabelAttribute>(property);
			return (labelAttribute == null) ? property.displayName : labelAttribute.Label;
		}

		public static object GetPropertyValue(SerializedProperty property)
		{
			switch (property.propertyType)
			{
				case SerializedPropertyType.Integer: return property.intValue;
				case SerializedPropertyType.Boolean: return property.boolValue;
				case SerializedPropertyType.Float: return property.floatValue;
				case SerializedPropertyType.String: return property.stringValue;
				case SerializedPropertyType.Color: return property.colorValue;
				case SerializedPropertyType.ObjectReference: return property.objectReferenceValue;
				case SerializedPropertyType.LayerMask: return property.intValue;
				case SerializedPropertyType.Enum: return property.enumValueIndex;
				case SerializedPropertyType.Vector2: return property.vector2Value;
				case SerializedPropertyType.Vector3: return property.vector3Value;
				case SerializedPropertyType.Vector4: return property.vector4Value;
				case SerializedPropertyType.Rect: return property.rectValue;
				case SerializedPropertyType.ArraySize: return property.intValue;
				case SerializedPropertyType.Character: return property.intValue;
				case SerializedPropertyType.AnimationCurve: return property.animationCurveValue;
				case SerializedPropertyType.Bounds: return property.boundsValue;
				case SerializedPropertyType.ExposedReference: return property.exposedReferenceValue;
				case SerializedPropertyType.Vector2Int: return property.vector2IntValue;
				case SerializedPropertyType.Vector3Int: return property.vector3IntValue;
				case SerializedPropertyType.RectInt: return property.rectIntValue;
				case SerializedPropertyType.BoundsInt: return property.boundsIntValue;
				case SerializedPropertyType.FixedBufferSize: return property.fixedBufferSize;
				case SerializedPropertyType.Quaternion: return property.quaternionValue;
				case SerializedPropertyType.ManagedReference: /*return property.managedReferenceValue;*/
				case SerializedPropertyType.Generic:
				case SerializedPropertyType.Gradient: /* return property.gradientValue; */
				default: throw new InvalidOperationException($"Property type ${property.type} is not supported");
			}
		}

		public static Type GetPropertyType(SerializedProperty property)
		{
			switch (property.propertyType)
			{
				case SerializedPropertyType.Integer: return typeof(int);
				case SerializedPropertyType.Boolean: return typeof(bool);
				case SerializedPropertyType.Float: return typeof(float);
				case SerializedPropertyType.String: return typeof(string);
				case SerializedPropertyType.Color: return typeof(Color);
				case SerializedPropertyType.ObjectReference: return typeof(object);
				case SerializedPropertyType.LayerMask: return typeof(int);
				case SerializedPropertyType.Enum: return typeof(Enum);
				case SerializedPropertyType.Vector2: return typeof(Vector2);
				case SerializedPropertyType.Vector3: return typeof(Vector3);
				case SerializedPropertyType.Vector4: return typeof(Vector4);
				case SerializedPropertyType.Rect: return typeof(Rect);
				case SerializedPropertyType.ArraySize: return typeof(int);
				case SerializedPropertyType.Character: return typeof(int);
				case SerializedPropertyType.AnimationCurve: return typeof(AnimationCurve);
				case SerializedPropertyType.Bounds: return typeof(Bounds);
				case SerializedPropertyType.ExposedReference: return typeof(Object);
				case SerializedPropertyType.Vector2Int: return typeof(Vector2Int);
				case SerializedPropertyType.Vector3Int: return typeof(Vector3Int);
				case SerializedPropertyType.RectInt: return typeof(RectInt);
				case SerializedPropertyType.BoundsInt: return typeof(BoundsInt);
				case SerializedPropertyType.FixedBufferSize: return typeof(int);
				case SerializedPropertyType.Quaternion: return typeof(Quaternion);
				case SerializedPropertyType.Gradient: /* return property.gradientValue; */
				case SerializedPropertyType.Generic:
				case SerializedPropertyType.ManagedReference:
				default: return null;
			}
		}

		public static void CallOnValueChangedCallbacks(SerializedProperty property)
		{
			var propertyType = GetPropertyType(property);
			if (propertyType == null)
				return;

			object target = GetTargetObjectWithProperty(property);
			object oldValue = GetPropertyValue(property);
			property.serializedObject.ApplyModifiedProperties(); // We must apply modifications so that the new value is updated in the serialized object
			object newValue = GetPropertyValue(property);
			var fieldName = property.name;

			OnValueChangedAttribute[] onValueChangedAttributes = GetAttributes<OnValueChangedAttribute>(property);
			foreach (var onValueChangedAttribute in onValueChangedAttributes)
			{
				MethodInfo callbackMethod = ReflectionUtility.GetMethod(target, onValueChangedAttribute.CallbackName);
				if (callbackMethod != null &&
				    callbackMethod.ReturnType == typeof(void) &&
				    callbackMethod.GetParameters().Length == 2)
				{
					ParameterInfo oldValueParam = callbackMethod.GetParameters()[0];
					ParameterInfo newValueParam = callbackMethod.GetParameters()[1];

					if (propertyType == oldValueParam.ParameterType &&
					    propertyType == newValueParam.ParameterType)
					{
						callbackMethod.Invoke(target, new object[] {oldValue, newValue});
					}
					else
					{
						string warning = string.Format(
							"The field '{0}' and the parameters of callback '{1}' must be of the same type." + Environment.NewLine +
							"Field={2}, Param0={3}, Param1={4}",
							fieldName, callbackMethod.Name, propertyType, oldValueParam.ParameterType, newValueParam.ParameterType);

						Debug.LogWarning(warning, property.serializedObject.targetObject);
					}
				}
				else
				{
					string warning = string.Format(
						"{0} can invoke only methods with 'void' return type and 2 parameters of the same type as the field the attribute was put on",
						onValueChangedAttribute.GetType().Name);

					Debug.LogWarning(warning, property.serializedObject.targetObject);
				}
			}
		}

		public static bool IsEnabled(SerializedProperty property)
		{
			EnableIfAttributeBase enableIfAttribute = GetAttribute<EnableIfAttributeBase>(property);
			if (enableIfAttribute == null)
			{
				return true;
			}

			var conditionValues = GetConditionValues(property, enableIfAttribute.Conditions);
			if (conditionValues.Count > 0)
			{
				bool enabled = GetConditionsFlag(conditionValues, enableIfAttribute.ConditionOperator, enableIfAttribute.Inverted);
				return enabled;
			}

			string message = enableIfAttribute.GetType().Name + " needs a valid boolean condition field, property or method name to work";
			Debug.LogWarning(message, property.serializedObject.targetObject);

			return false;
		}

		public static bool IsVisible(SerializedProperty property)
		{
			ShowIfAttributeBase showIfAttribute = GetAttribute<ShowIfAttributeBase>(property);
			if (showIfAttribute == null)
			{
				return true;
			}

			List<bool> conditionValues = GetConditionValues(property, showIfAttribute.Conditions);
			if (conditionValues.Count > 0)
			{
				bool enabled = GetConditionsFlag(conditionValues, showIfAttribute.ConditionOperator, showIfAttribute.Inverted);
				return enabled;
			}

			string message = showIfAttribute.GetType().Name + " needs a valid boolean condition field, property or method name to work";
			Debug.LogWarning(message, property.serializedObject.targetObject);

			return false;
		}

		private static List<bool> GetConditionValues(SerializedProperty property, string[] conditions)
		{
			var serializedObject = property.serializedObject;
			List<bool> conditionValues = new List<bool>();
			var indexOfDot = property.propertyPath.LastIndexOf('.');
			var outerPath = indexOfDot == -1 ? "" : property.propertyPath.Substring(0, indexOfDot);
			foreach (var condition in conditions)
			{
				var conditionPath = outerPath.Length == 0 ? condition : outerPath + "." + condition;
				var conditionProperty = serializedObject.FindProperty(conditionPath);
				if (conditionProperty != null && conditionProperty.propertyType == SerializedPropertyType.Boolean)
					conditionValues.Add(conditionProperty.boolValue);
				else
				{
					var target = GetTargetObjectWithProperty(property);

					var reflectionProperty = ReflectionUtility.GetProperty(target, condition);
					if (reflectionProperty != null &&
					    reflectionProperty.PropertyType == typeof(bool))
					{
						conditionValues.Add((bool) reflectionProperty.GetValue(target));
					}
					else
					{
						var reflectionMethod = ReflectionUtility.GetMethod(target, condition);
						if (reflectionMethod != null &&
						    reflectionMethod.ReturnType == typeof(bool) &&
						    reflectionMethod.GetParameters().Length == 0)
						{
							conditionValues.Add((bool) reflectionMethod.Invoke(target, null));
						}
					}
				}
			}

			return conditionValues;
		}

		private static bool GetConditionsFlag(List<bool> conditionValues, EConditionOperator conditionOperator, bool invert)
		{
			bool flag;
			switch (conditionOperator)
			{
				case EConditionOperator.And:
				{
					flag = true;
					foreach (var value in conditionValues)
						flag = flag && value;
					return invert ? !flag : flag;
				}
				case EConditionOperator.Or:
				{
					flag = false;
					foreach (var value in conditionValues)
						flag = flag || value;
					return invert ? !flag : flag;
				}
				default:
					throw new InvalidOperationException($"Unknown conditional operator {conditionOperator}");
			}
		}

		/// <summary>
		/// Gets the object the property represents.
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public static object GetTargetObjectOfProperty(SerializedProperty property)
		{
			if (property == null)
			{
				return null;
			}

			string path = property.propertyPath.Replace(".Array.data[", "[");
			object obj = property.serializedObject.targetObject;
			string[] elements = path.Split('.');

			foreach (var element in elements)
			{
				if (element.Contains("["))
				{
					var indexOfBracket = element.IndexOf("[", StringComparison.InvariantCulture);
					string elementName = element.Substring(0, indexOfBracket);
					int index = Convert.ToInt32(element.Substring(indexOfBracket).Replace("[", "").Replace("]", ""));
					obj = GetValue_Imp(obj, elementName, index);
				}
				else
				{
					obj = GetValue_Imp(obj, element);
				}
			}

			return obj;
		}

		/// <summary>
		/// Gets the object that the property is a member of
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public static object GetTargetObjectWithProperty(SerializedProperty property)
		{
			string path = property.propertyPath.Replace(".Array.data[", "[");
			object obj = property.serializedObject.targetObject;
			string[] elements = path.Split('.');

			for (int i = 0; i < elements.Length - 1; i++)
			{
				string element = elements[i];
				if (element.Contains("["))
				{
					var indexOfBracket = element.IndexOf("[", StringComparison.InvariantCulture);
					string elementName = element.Substring(0, indexOfBracket);
					int index = Convert.ToInt32(element.Substring(indexOfBracket).Replace("[", "").Replace("]", ""));
					obj = GetValue_Imp(obj, elementName, index);
				}
				else
				{
					obj = GetValue_Imp(obj, element);
				}
			}

			return obj;
		}

		private static object GetValue_Imp(object source, string name)
		{
			if (source == null)
			{
				return null;
			}

			Type type = source.GetType();

			while (type != null)
			{
				FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				if (field != null)
				{
					return field.GetValue(source);
				}

				PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				if (property != null)
				{
					return property.GetValue(source, null);
				}

				type = type.BaseType;
			}

			return null;
		}

		private static object GetValue_Imp(object source, string name, int index)
		{
			IEnumerable enumerable = GetValue_Imp(source, name) as IEnumerable;
			if (enumerable == null)
			{
				return null;
			}

			IEnumerator enumerator = enumerable.GetEnumerator();
			for (int i = 0; i <= index; i++)
			{
				if (!enumerator.MoveNext())
				{
					return null;
				}
			}

			return enumerator.Current;
		}
	}
}