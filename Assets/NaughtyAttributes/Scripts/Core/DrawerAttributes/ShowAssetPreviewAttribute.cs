using System;
using UnityEngine;

namespace NaughtyAttributes
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class ShowAssetPreviewAttribute : DrawerAttribute
	{
		public int Width { get; private set; }
		public int Height { get; private set; }
		public TextAlignment Alignment = TextAlignment.Left;
		
		public ShowAssetPreviewAttribute(int width = 64, int height = 64)
		{
			Width = width;
			Height = height;
		}
	}
}
