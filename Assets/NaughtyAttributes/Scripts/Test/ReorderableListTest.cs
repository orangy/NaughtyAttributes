using System.Collections.Generic;
using UnityEngine;

namespace NaughtyAttributes.Test
{
	public class ReorderableListTest : MonoBehaviour
	{
		[ReorderableList]
		public int[] intArray;

		[ReorderableList]
		public List<Vector3> vectorList;

		[ReorderableList]
		public List<SomeStruct> structList;

		[ReorderableList]
		[ShowAssetPreview]
		public List<GameObject> prefabList;
	}

	[System.Serializable]
	public struct SomeStruct
	{
		public int Int;
		public float Float;
		public Vector3 Vector;
	}
}
