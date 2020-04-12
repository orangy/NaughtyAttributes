using System;

namespace NaughtyAttributes
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class ButtonAttribute : SpecialCaseDrawerAttribute
	{
		public string Text { get; }
		public float SpaceBefore = 0;
		public float SpaceAfter = 0;

		public ButtonAttribute(string text = null)
		{
			Text = text;
		}
	}
}
