using HugsLib.Settings;
using UnityEngine;
using Verse;

namespace BlueprintTotalsTooltip.TotalsTipSettingsUtilities
{
	enum RectDimensionPosition : int
	{
		LowerWithLowerOffset = 0,
		LowerCenter = 1,
		LowerWithHigherOffset = 2,
		CenterWithLowerOffset = 3,
		Center = 4,
		CenterWithHigherOffset = 5,
		HigherWithLowerOffset = 6,
		HigherCenter = 7,
		HigherWithHigherOffset = 8
	}

	static class TipPosSettingsHandler
	{
		private static readonly float marginSize = 3f;

		public static bool settingsChanged = false;

		public static void DrawTipPosSetting(Rect rect, SettingHandle<int> xHandler, SettingHandle<int> yHandler)
		{
			settingsChanged = false;
			xHandler.CustomDrawerHeight = rect.width;
			Rect drawingRect = rect.ContractedBy(marginSize);
			int[] valuesFromSliders = DrawTwoAxisIntSliders(drawingRect, 16f, new int[] { xHandler.Value, yHandler.Value });
			if (valuesFromSliders[0] != xHandler.Value || valuesFromSliders[1] != yHandler.Value)
			{
				xHandler.Value = valuesFromSliders[0];
				yHandler.Value = valuesFromSliders[1];
				settingsChanged = true;
			}
			DoToolTip(rect);
			DrawHelperGraphics(drawingRect, xHandler, yHandler, 16f);
		}

		private static void DoToolTip(Rect settingsRect)
		{
			Rect tooltipRect = new Rect(settingsRect.x - settingsRect.width, settingsRect.y, settingsRect.width, settingsRect.height);
			TooltipHandler.TipRegion(tooltipRect, new TipSignal("tooltipPosition_desc".Translate()));
		}

		private static int[] DrawTwoAxisIntSliders(Rect rect, float sliderWidth, int[] values)
		{
			GUI.BeginGroup(rect);
			Rect horizontalRect = new Rect(sliderWidth, 0f, rect.width - sliderWidth, sliderWidth);
			Rect verticalRect = new Rect(0f, sliderWidth, sliderWidth, rect.height - sliderWidth);
			int xSliderValue = (int)Widgets.HorizontalSlider(horizontalRect, values[0], 0, 8, true, null, null, null, 1);
			int ySliderValue = (int)ToolTipSettingsUtility.LabelLessVerticalSlider(verticalRect, values[1], 0, 8, true, 1);
			GUI.EndGroup();
			return new int[] { xSliderValue, ySliderValue };
		}

		private static void DrawHelperGraphics(Rect rect, SettingHandle<int> xHandler, SettingHandle<int> yHandler, float sliderWidth)
		{
			Rect blueprints = new Rect(rect.x + sliderWidth, rect.y + sliderWidth, rect.width - sliderWidth, rect.height - sliderWidth).ContractedBy(16 * marginSize);
			Widgets.DrawHighlight(blueprints);
			float helperTipSize = 12 * marginSize;
			float tooltipX = GetDimensionFromSetting(blueprints.xMin, blueprints.xMax, helperTipSize, (RectDimensionPosition)xHandler.Value);
			float tooltipY = GetDimensionFromSetting(blueprints.yMin, blueprints.yMax, helperTipSize, (RectDimensionPosition)yHandler.Value);
			Rect tooltip = new Rect(tooltipX, tooltipY, helperTipSize, helperTipSize);
			Widgets.DrawBox(tooltip);
			DrawCenterlines(blueprints.center.x, blueprints.center.y, helperTipSize + blueprints.width / 2);
		}

		public static float GetDimensionFromSetting(float lower, float upper, float dimWidth, RectDimensionPosition setting)
		{
			switch (setting)
			{
				case RectDimensionPosition.LowerWithLowerOffset:
					return lower - dimWidth;
				case RectDimensionPosition.LowerCenter:
					return lower - dimWidth / 2;
				case RectDimensionPosition.LowerWithHigherOffset:
					return lower;
				case RectDimensionPosition.CenterWithLowerOffset:
					return 0.5f * (lower + upper) - dimWidth;
				case RectDimensionPosition.Center:
					return 0.5f * (lower + upper) - dimWidth / 2;
				case RectDimensionPosition.CenterWithHigherOffset:
					return 0.5f * (lower + upper);
				case RectDimensionPosition.HigherWithLowerOffset:
					return upper - dimWidth;
				case RectDimensionPosition.HigherCenter:
					return upper - dimWidth / 2;
				case RectDimensionPosition.HigherWithHigherOffset:
					return upper;
			}
			return 0;
		}

		private static void DrawCenterlines(float centerX, float centerY, float length)
		{
			Widgets.DrawLineHorizontal(centerX - length, centerY, 2 * length);
			Widgets.DrawLineVertical(centerX, centerY - length, 2 * length);
		}
	}
}
