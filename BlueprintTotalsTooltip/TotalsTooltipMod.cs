using BlueprintTotalsTooltip.TotalsTipSettingsUtilities;
using HugsLib;
using HugsLib.Settings;
using UnityEngine;
using Verse;

namespace BlueprintTotalsTooltip
{
	class TotalsTooltipMod : ModBase
	{
		#region settings
		public SettingHandle<bool> TrackingVisible { get; private set; }
		public SettingHandle<CameraZoomRange> ZoomForVisibleTracking { get; private set; }
		public SettingHandle<int> VisibilityMargin { get; private set; }
		public SettingHandle<bool> ClampTipToScreen { get; private set; }
		public SettingHandle<int> TooltipClampMargin { get; private set; }
		public SettingHandle<float> HighlightOpacity { get; private set; }
		public SettingHandle<bool> ShowRowToolTips { get; private set; }
		public SettingHandle<bool> CountInStorage { get; private set; }
		public SettingHandle<bool> CountForbidden { get; private set; }
		public SettingHandle<int> TipXPosition { get; private set; }
		public SettingHandle<int> TipYPosition { get; private set; }
		#endregion settings

		public TotalsTooltipDrawer TotalsTipDrawer { get; }

		public override string ModIdentifier
		{
			get { return "BlueprintTotalsTooltip"; }
		}

		public TotalsTooltipMod()
		{
			TotalsTipDrawer = new TotalsTooltipDrawer(this);
		}

		public override void SettingsChanged()
		{
			TotalsTipDrawer.ResolveSettings();
			AssetLoader.trackedHighlightTexture.Apply();
		}

		public override void DefsLoaded()
		{
			TrackingVisible = Settings.GetHandle("trackingVisible", "trackingVisible_title".Translate(), "trackingVisible_desc".Translate(), true);
			ZoomForVisibleTracking = Settings.GetHandle("zoomForTracking", "zoomForTracking_title".Translate(), "zoomForTracking_desc".Translate(),
				CameraZoomRange.Middle, null, "zoomForTracking_");
			VisibilityMargin = Settings.GetHandle("visibilityMargin", "visibilityMargin_title".Translate(), "visibilityMargin_desc".Translate(), 100,
				Validators.IntRangeValidator(0, UI.screenHeight / 2));
			VisibilityMargin.SpinnerIncrement = 10;
			ClampTipToScreen = Settings.GetHandle("clampTipToScreen", "clampTipToScreen_title".Translate(), "clampTipToScreen_desc".Translate(), true);
			TooltipClampMargin = Settings.GetHandle("clampMargin", "clampMargin_title".Translate(), "clampMargin_desc".Translate(), 10,
				Validators.IntRangeValidator(0, UI.screenHeight / 2));
			TooltipClampMargin.SpinnerIncrement = 10;
			HighlightOpacity = Settings.GetHandle("highlightOpacity", "highlightOpacity_title".Translate(), "highlightOpacity_desc".Translate(), 0.10f);
			HighlightOpacity.CustomDrawer = OpacityCustomDrawer;
			ShowRowToolTips = Settings.GetHandle("showTips", "showTips_title".Translate(), "showTips_desc".Translate(), true);
			CountInStorage = Settings.GetHandle("countInStorage", "countInStorage_title".Translate(), "countInStorage_desc".Translate(), false);
			CountForbidden = Settings.GetHandle("countForbidden", "countForbidden_title".Translate(), "countForbidden_desc".Translate(), false);
			ResolveTipPositionHandlers();
			TotalsTipDrawer.ResolveSettings();
		}

		public override void OnGUI()
		{
			TotalsTipDrawer.OnGUI();
		}

		private void ResolveTipPositionHandlers()
		{
			TipXPosition = Settings.GetHandle("tooltipXPosition", "tooltipPosition_title".Translate(), null, 8);
			TipXPosition.CustomDrawer = TipXCustomDrawer;
			TipYPosition = Settings.GetHandle("tooltipYPosition", "", "", 2);
			TipYPosition.CustomDrawer = TipXCustomDrawer;
			TipYPosition.VisibilityPredicate = delegate { return false; };
		}
		private bool OpacityCustomDrawer(Rect rect)
		{
			Rect sliderRect = new Rect(rect.x, rect.y, rect.width - 3f, rect.height);
			float horizontalSliderResult = Widgets.HorizontalSlider(sliderRect, HighlightOpacity.Value, 0f, 0.25f, true, null, null, null, 1f / 100f);
			if (horizontalSliderResult != HighlightOpacity.Value)
			{
				HighlightOpacity.Value = horizontalSliderResult;
				return true;
			}
			return false;
		}

		private bool TipXCustomDrawer(Rect rect)
		{
			TipPosSettingsHandler.DrawTipPosSetting(rect, TipXPosition, TipYPosition);
			return TipPosSettingsHandler.settingsChanged;
		}
	}
}
