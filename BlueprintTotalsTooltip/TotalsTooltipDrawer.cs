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
		private bool highlightEnabled;

		private RectDimensionPosition tipXPos;
		private RectDimensionPosition tipYPos;

		private readonly float highlightMargin = 5f;
		private readonly float listElementsMargin = 3f;
		private readonly float listElementsHeight = 29f;
		private readonly float xOffsetFromContainer = 10f;

		private TotalsTooltipMod modInstance;
		private CameraChangeDetector cameraChangeDetector;

		public static bool shouldDraw = false;

		private bool zoomWasValid = false;

		public bool ZoomIsValid { get { return (int)Find.CameraDriver.CurrentZoom <= (int)modInstance.ZoomForVisibleTracking.Value; } }

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
			FrameWorkedOnDetector.RegisterMethod(Tracker.FrameBeingBuilt);
			cameraChangeDetector = new CameraChangeDetector();
			cameraChangeDetector.RegisterMethod(OnCameraChange);
		}

		public void ResolveSettings()
		{
			highlightEnabled = modInstance.HighlightOpacity.Value != 0f;
			tipXPos = (RectDimensionPosition)modInstance.TipXPosition.Value;
			tipYPos = (RectDimensionPosition)modInstance.TipYPosition.Value;
			Tracker.ResolveSettings(modInstance);
			OnPlaySettingChange();
		}

		#region callbacks
		public void OnPlaySettingChange()
		{
			if (shouldDraw && !WorldRendererUtility.WorldRenderedNow)
			{
				if (Find.Selector.NumSelected == 0 && ZoomIsValid && modInstance.TrackingVisible)
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
				if (Find.Selector.NumSelected == 0 && modInstance.TrackingVisible)
					Tracker.TrackVisibleConstructibles();
				else
					Tracker.TrackSelectedConstructibles();
			}
		}

		public void OnCameraChange()
		{
			if (shouldDraw && modInstance.TrackingVisible)
			{
				if (Find.Selector.NumSelected == 0)
				{
					if (ZoomIsValid)
					{
						Tracker.TrackVisibleConstructibles();
					}
					else if (zoomWasValid)
					{
						Tracker.ClearTracked();
					}
				}
				zoomWasValid = ZoomIsValid;
			}
		}

		public void OnThingAdded(Thing thing)
		{
			if (Find.Selector.NumSelected == 0 && modInstance.TrackingVisible && shouldDraw && !WorldRendererUtility.WorldRenderedNow)
				if (thing is IConstructible)
				{
					Tracker.TryTrackConstructible(thing);
				}
		}

		public void OnThingRemove(Thing thing)
		{
			if (Find.Selector.NumSelected == 0 && modInstance.TrackingVisible && shouldDraw && !WorldRendererUtility.WorldRenderedNow)
			{
				if (thing is IConstructible)
				{
					Tracker.TryUntrackConstructible(thing);
				}
			}
		}
		#endregion callbacks

		public void OnGUI()
		{
			if (Find.CurrentMap != null && !WorldRendererUtility.WorldRenderedNow)
			{
				cameraChangeDetector.OnGUI();
				if (shouldDraw && Tracker.NumberTracked > 0)
				{
					if (highlightEnabled && Find.Selector.NumSelected == 0) Tracker.HighlightTracked(modInstance.HighlightOpacity, highlightMargin);
					DrawToolTip();
				}
			}
		}

		private void DrawToolTip()
		{
			List<ThingDefCount> trackedRequirements = Tracker.TotalCosts;
			float maxCountWidth = (trackedRequirements.Count > 0) ? Text.CalcSize(trackedRequirements[0].Count.ToString()).x : 0f;
			float workLeftWidth = Text.CalcSize(Tracker.WorkLeft.ToString()).x;
			float toolTipWidth = Mathf.Max(maxCountWidth, workLeftWidth);
			toolTipWidth += (listElementsMargin * 2 + xOffsetFromContainer * 2) + listElementsHeight;
			float toolTipHeight = (trackedRequirements.Count + 1) * listElementsHeight;
			toolTipHeight += listElementsMargin * 2;
			Rect tooltipRect = new Rect(0f, 0f, toolTipWidth, toolTipHeight);
			PositionTipRect(ref tooltipRect);
			if (modInstance.ClampTipToScreen)
			{
				tooltipRect = tooltipRect.ClampRectInRect(new Rect(0, 0, UI.screenWidth, UI.screenHeight).ContractedBy(modInstance.TooltipClampMargin));
			}
			Rect innerTipRect = tooltipRect.ContractedBy(listElementsMargin).WidthContractedBy(xOffsetFromContainer);
			int indexOffset = 0;
			if (Find.Selector.NumSelected > 0)
			{
				DrawTrackingModeIndicator(innerTipRect, indexOffset);
				indexOffset = 1;
			}
			for (int i = 0; i < trackedRequirements.Count; i++)
			{
				ThingDefCount count = trackedRequirements[i];
				DrawRequirementRow(count, innerTipRect, i + indexOffset);
			}
			DrawWorkLeftRow(innerTipRect, trackedRequirements.Count + indexOffset);
		}

		private void PositionTipRect(ref Rect tooltipRect)
		{
			Rect containingRect = Tracker.ContainingRect;
			float xPos = TipPosSettingsHandler.GetDimensionFromSetting(containingRect.xMin, containingRect.xMax, tooltipRect.width, tipXPos);
			float yPos = TipPosSettingsHandler.GetDimensionFromSetting(containingRect.yMin, containingRect.yMax, tooltipRect.height, tipYPos);
			tooltipRect.position = new Vector2(xPos, yPos);
		}

		private void DrawTrackingModeIndicator(Rect toolTipRect, int posInList)
		{
			Rect rowRect = new Rect(toolTipRect.x, toolTipRect.y + posInList * listElementsHeight, toolTipRect.width, listElementsHeight);
			Rect iconRect = new Rect(rowRect.x, rowRect.y, listElementsHeight, listElementsHeight).ContractedBy(1.5f);
			RectUtility.DrawBracketsAroundRect(iconRect);
		}

		private void DrawRequirementRow(ThingDefCount count, Rect toolTipRect, int posInList)
		{
			Rect rowRect = new Rect(toolTipRect.x, toolTipRect.y + posInList * listElementsHeight, toolTipRect.width, listElementsHeight);
			Rect iconRect = new Rect(rowRect.x, rowRect.y, listElementsHeight, listElementsHeight).ContractedBy(1.5f);
			Widgets.ThingIcon(iconRect, count.ThingDef);
			Rect labelRect = new Rect(rowRect.x + listElementsHeight, rowRect.y, rowRect.width - listElementsHeight, rowRect.height);
			Text.Anchor = TextAnchor.MiddleLeft;
			int difference = (modInstance.CountInStorage) ? Find.CurrentMap.GetCountInStorageDifference(count) : Find.CurrentMap.GetCountOnMapDifference(count, modInstance.CountForbidden);
			if (difference > 0) GUI.color = Color.red;
			Widgets.Label(labelRect, count.Count.ToString());
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			if (modInstance.ShowRowToolTips) DoRowTooltip(rowRect, count, difference);
		}

		private void DoRowTooltip(Rect tooltipRegion, ThingDefCount count, int difference)
		{
			int present = -difference + count.Count;
			string tipLabel = (modInstance.CountInStorage) ? "ReqRowTip_Storage".Translate(count.Count, present) : "ReqRowTip_All".Translate(count.Count, present);
			TooltipHandler.TipRegion(tooltipRegion, new TipSignal(tipLabel));
		}

		private void DrawWorkLeftRow(Rect toolTipRect, int posInList)
		{
			Rect rowRect = new Rect(toolTipRect.x, toolTipRect.y + posInList * listElementsHeight, toolTipRect.width, listElementsHeight);
			Rect iconRect = new Rect(rowRect.x, rowRect.y, listElementsHeight, listElementsHeight).ContractedBy(1.5f);
			GUI.DrawTexture(iconRect, AssetLoader.workLeftTexture);
			Rect labelRect = new Rect(rowRect.x + listElementsHeight, rowRect.y, rowRect.width - listElementsHeight, rowRect.height);
			Text.Anchor = TextAnchor.MiddleLeft;
			string workLeftAsString = Tracker.WorkLeft.ToString();
			Widgets.Label(labelRect, workLeftAsString);
			DoWorkLeftTooltip(rowRect, workLeftAsString);
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void DoWorkLeftTooltip(Rect tipRegion, string workLeftAsString)
		{
			TooltipHandler.TipRegion(tipRegion, new TipSignal("WorkLeftTip".Translate(workLeftAsString)));
		}
	}
}