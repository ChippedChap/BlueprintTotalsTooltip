using BlueprintTotalsTooltip.TotalsTipUtilities;
using BlueprintTotalsTooltip.ChangeDetection;
using BlueprintTotalsTooltip.SelectorChangeNotifiers;
using BlueprintTotalsTooltip.FrameChangeNotifiers;
using BlueprintTotalsTooltip.LTChangeNotifiers;
using BlueprintTotalsTooltip.TotalsTipSettingsUtilities;
using System.Collections.Generic;
using UnityEngine;
using RimWorld.Planet;
using RimWorld;
using Verse;

namespace BlueprintTotalsTooltip
{
	class TotalsTooltipDrawer
	{
		#region player settings
		private bool clampTip;
		private float clampMargin;
		private CameraZoomRange maximumZoom;
		private bool trackingVisible;
		private bool tooltipsEnabled;
		private bool countStorage;
		private bool countForbiddenItems;

		private RectDimensionPosition xPosition;
		private RectDimensionPosition yPosition;
		#endregion player settings

		private readonly float highlightMargin = 5f;
		private readonly float listElementsMargin = 3f;
		private readonly float listElementsHeight = 29f;
		private readonly float xOffsetFromContainer = 10f;

		private TotalsTooltipMod modInstance;
		private CameraChangeDetector cameraChangeDetector;

		public static bool shouldDraw = false;

		private bool zoomWasValid = false;

		public bool ZoomIsValid { get { return (int)Find.CameraDriver.CurrentZoom <= (int)maximumZoom; } }

		public ConstructibleTotalsTracker Tracker { get; }

		public TotalsTooltipDrawer(TotalsTooltipMod mod)
		{
			modInstance = mod;
			Tracker = new ConstructibleTotalsTracker();
			SelectionChangeNotifierData.RegisterMethod(OnSelectionChange);
			FrameChangeNotifierData.RegisterMethod(OnSelectionChange);
			PlaySettingsChangeDetector.RegisterMethod(OnPlaySettingChange);
			LTAddNotifier.RegisterMethod(OnThingAdded);
			LTRemoveNotifier.RegisterMethod(OnThingRemove);
			cameraChangeDetector = new CameraChangeDetector();
			cameraChangeDetector.RegisterMethod(OnCameraChange);
		}

		public void ResolveSettings()
		{
			clampTip = modInstance.ClampTipToScreen;
			clampMargin = modInstance.TooltipClampMargin;
			trackingVisible = modInstance.TrackingVisible;
			maximumZoom = modInstance.ZoomForVisibleTracking;
			tooltipsEnabled = modInstance.ShowRowToolTips;
			countStorage = modInstance.CountInStorage;
			countForbiddenItems = modInstance.CountForbidden;
			xPosition = (RectDimensionPosition)modInstance.TipXPosition.Value;
			yPosition = (RectDimensionPosition)modInstance.TipYPosition.Value;
			AssetLoader.trackedHighlightTexture.SetPixel(0, 0, new Color(1f, 1f, 1f, modInstance.HighlightOpacity));
			Tracker.ResolveSettings(modInstance);
			OnPlaySettingChange();
		}

		#region callbacks
		public void OnPlaySettingChange()
		{
			if (shouldDraw && !WorldRendererUtility.WorldRenderedNow)
			{
				if (Find.Selector.NumSelected == 0 && ZoomIsValid && trackingVisible)
				{
					Tracker.TrackVisibleConstructibles();
				}
				else
				{
					Tracker.ClearTracked();
					Tracker.TrackSelectedConstructibles();
				}
			}
		}

		public void OnSelectionChange()
		{
			if (shouldDraw && !WorldRendererUtility.WorldRenderedNow)
			{
				if (Find.Selector.NumSelected == 0 && trackingVisible)
					Tracker.TrackVisibleConstructibles();
				else
					Tracker.TrackSelectedConstructibles();
			}
		}

		public void OnCameraChange()
		{
			if (shouldDraw && trackingVisible)
			{
				if (Find.Selector.NumSelected == 0)
				{
					if (ZoomIsValid)
						Tracker.TrackVisibleConstructibles();
					else if (zoomWasValid)
						Tracker.ClearTracked();
				}
				zoomWasValid = ZoomIsValid;
			}
		}

		public void OnThingAdded(Thing thing)
		{
			if (Find.Selector.NumSelected == 0 && trackingVisible && shouldDraw && !WorldRendererUtility.WorldRenderedNow)
				if (thing is IConstructible)
				{
					Tracker.TryTrackConstructible(thing);
				}
		}

		public void OnThingRemove(Thing thing)
		{
			if (Find.Selector.NumSelected == 0 && trackingVisible && shouldDraw && !WorldRendererUtility.WorldRenderedNow)
				if (thing is IConstructible)
					Tracker.TryUntrackConstructible(thing);
		}
		#endregion callbacks

		public void OnGUI()
		{
			if (Find.VisibleMap != null && !WorldRendererUtility.WorldRenderedNow)
			{
				cameraChangeDetector.OnGUI();
				if (Tracker.NumberTracked > 0 && shouldDraw)
				{
					if (AssetLoader.trackedHighlightTexture.GetPixel(1, 1).a != 0)
						Tracker.HighlightTracked(AssetLoader.trackedHighlightTexture, highlightMargin);
					DrawToolTip();
				}
			}
		}

		private void DrawToolTip()
		{
			List<ThingCount> trackedRequirements = Tracker.GetTrackedTotals();
			if (trackedRequirements.Count > 0)
			{
				float toolTipWidth = Text.CalcSize(trackedRequirements[0].Count.ToString()).x + listElementsHeight;
				toolTipWidth += (listElementsMargin * 2 + xOffsetFromContainer * 2);
				float toolTipHeight = (trackedRequirements.Count) * listElementsHeight;
				toolTipHeight += listElementsMargin * 2;
				Rect tooltipRect = new Rect(0f, 0f, toolTipWidth, toolTipHeight);
				PositionTipRect(ref tooltipRect);
				if (clampTip)
					tooltipRect = tooltipRect.ClampRectInRect(new Rect(0, 0, UI.screenWidth, UI.screenHeight).ContractedBy(clampMargin));
				Rect innerTipRect = tooltipRect.ContractedBy(listElementsMargin).WidthContractedBy(xOffsetFromContainer);
				for (int i = 0; i < trackedRequirements.Count; i++)
				{
					ThingCount count = trackedRequirements[i];
					DrawRequirementRow(count, innerTipRect, i);
				}
			}
		}

		private void PositionTipRect(ref Rect tooltipRect)
		{
			Rect containingRect = Tracker.ContainingRect;
			float xPos = TipPosSettingsHandler.GetDimensionFromSetting(containingRect.xMin, containingRect.xMax, tooltipRect.width, xPosition);
			float yPos = TipPosSettingsHandler.GetDimensionFromSetting(containingRect.yMin, containingRect.yMax, tooltipRect.height, yPosition);
			tooltipRect.position = new Vector2(xPos, yPos);
		}

		private void DrawRequirementRow(ThingCount count, Rect toolTipRect, int posInList)
		{
			Rect rowRect = new Rect(toolTipRect.x, toolTipRect.y + posInList * listElementsHeight, toolTipRect.width, listElementsHeight);
			Rect iconRect = new Rect(rowRect.x, rowRect.y, listElementsHeight, listElementsHeight).ContractedBy(1.5f);
			Widgets.ThingIcon(iconRect, count.ThingDef);
			Rect labelRect = new Rect(rowRect.x + listElementsHeight, rowRect.y, rowRect.width - listElementsHeight, rowRect.height);
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.Font = GameFont.Small;
			int difference = (countStorage) ? Find.VisibleMap.GetCountInStorageDifference(count) : Find.VisibleMap.GetCountOnMapDifference(count, countForbiddenItems);
			if (difference > 0) GUI.color = Color.red;
			Widgets.Label(labelRect, count.Count.ToString());
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			if (tooltipsEnabled) DoRowTooltip(rowRect, count, difference);
		}

		private void DoRowTooltip(Rect tooltipRegion, ThingCount count, int difference)
		{
			int present = -difference + count.Count;
			object[] translateArgs = new object[] { count.Count, present };
			string tipLabel = (countStorage) ? "ReqRowTip_Storage".Translate(translateArgs) : "ReqRowTip_All".Translate(translateArgs);
			TooltipHandler.TipRegion(tooltipRegion, new TipSignal(tipLabel));
		}
	}
}
