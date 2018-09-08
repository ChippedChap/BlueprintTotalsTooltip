using UnityEngine;
using Verse;

namespace BlueprintTotalsTooltip.TotalsTipSettingsUtilities
{
	static class ToolTipSettingsUtility
	{
		public static float LabelLessVerticalSlider(Rect rect, float value, float min, float max, bool middleAlignment = true, float roundTo = -1f)
		{
			if (middleAlignment)
			{
				rect.x += Mathf.Round((rect.width - 16f) / 2f);
			}
			float num = GUI.VerticalSlider(rect, value, min, max);
			if (roundTo > 0f)
			{
				num = Mathf.RoundToInt(num / roundTo) * roundTo;
			}
			return num;
		}
	}
}
