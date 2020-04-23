using BlueprintTotalsTooltip.TotalsTipUtilities;
using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;

namespace BlueprintTotalsTooltip
{
	class ConstructibleTotalsTracker
	{
		public bool trackForbidden;
		public float visibleMargin;

		private HashSet<IConstructible> trackedConstructibles = new HashSet<IConstructible>();
		private List<ThingDefCount> cachedCosts;
		private bool cacheUpdated = false;

		private Dictionary<Frame, int> cachedWorkLeftForFrames = new Dictionary<Frame, int>();
		private CellRect containingRect = CellRect.Empty;
		private Rect highlightRect = Rect.zero;

		public int NumberTracked => trackedConstructibles.Count;

		public Rect ContainingRect
		{
			get
			{
				if (containingRect.Equals(CellRect.Empty))
				{
					if (TotalsTooltipDrawer.NoConstructablesSelected)
						TrackVisibleConstructibles();
					else
						TrackSelectedConstructibles();
				}
				return containingRect.ToScreenRect();
			}
		}

		public int WorkLeft { get; private set; } = 0;

		public List<ThingDefCount> TotalCosts
		{
			get
			{
				if (!cacheUpdated)
				{
					List<ThingDefCount> protoCosts = new List<ThingDefCount>();
					foreach (IConstructible constructible in trackedConstructibles)
					{
						for (int i = 0; i < constructible.MaterialsNeededSafe().Count; i++)
						{
							ThingDefCount matCount = constructible.MaterialsNeededSafe()[i];
							int sameThingDefIndex = protoCosts.FindIndex(x => x.ThingDef == matCount.ThingDef);
							if (sameThingDefIndex > -1)
								protoCosts[sameThingDefIndex] = ConstructibleUtility.AddThingDefCounts(protoCosts[sameThingDefIndex], matCount);
							else
								protoCosts.Add(matCount);
						}
					}
					cachedCosts = protoCosts;
					cachedCosts.Sort((x, y) => y.Count.CompareTo(x.Count));
				}
				cacheUpdated = true;
				return cachedCosts;
			}
		}

		public void ClearTracked() => trackedConstructibles.Clear();

		public void ResolveSettings(TotalsTooltipMod mod)
		{
			trackForbidden = mod.TrackingForbidden;
			visibleMargin = mod.VisibilityMargin;
		}

		#region track updating
		public void TryTrackConstructible(Thing constructible)
		{
			if (trackedConstructibles.Contains(constructible as IConstructible) ||
				!constructible.ThingIsVisible(visibleMargin, out Vector3[] corners) &&
				!Find.Selector.SelectedObjects.Contains(constructible)) return;
			Track(constructible as IConstructible);
			for (int i = 0; i < corners.Length; i++)
			{
				if (containingRect.Equals(CellRect.Empty))
					containingRect = CellRect.FromLimits((int)corners[i].x, (int)corners[i].z, (int)corners[i].x, (int)corners[i].z);
				else
					containingRect = containingRect.ExpandToContain(corners[i]);
			}
			cacheUpdated = false;
		}

		public void TryUntrackConstructible(Thing constructible)
		{
			if (trackedConstructibles.Contains(constructible as IConstructible))
			{
				Untrack(constructible as IConstructible);
				CellRectBuilder rectBuilder = new CellRectBuilder();
				foreach (IConstructible tracked in trackedConstructibles)
				{
					Vector3[] corners = (tracked as Thing).CornerPositions();
					for (int i = 0; i < corners.Length; i++) rectBuilder.ConsiderPoint(corners[i]);
				}
				containingRect = rectBuilder.ToCellRect();
				cacheUpdated = false;
			}
		}

		public void TrackVisibleConstructibles()
		{
			List<Thing> candidateThings = Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Construction);
			trackedConstructibles.Clear();
			cachedWorkLeftForFrames.Clear();
			WorkLeft = 0;
			CellRectBuilder crBuilder = new CellRectBuilder();
			for (int i = 0; i < candidateThings.Count; i++)
			{
				Thing candidateThing = candidateThings[i];
				if (ForbidUtility.IsForbidden(candidateThing, Faction.OfPlayer) && !trackForbidden) continue;
				if (!candidateThing.ThingIsVisible(visibleMargin, out Vector3[] corners)) continue;
				Track(candidateThing as IConstructible);
				for (int j = 0; j < corners.Length; j++) crBuilder.ConsiderPoint(corners[j]);
			}
			containingRect = crBuilder.ToCellRect();
			cacheUpdated = false;
		}

		public void TrackSelectedConstructibles()
		{
			cachedWorkLeftForFrames.Clear();
			WorkLeft = 0;
			foreach (object selected in Find.Selector.SelectedObjects)
			{
				if (selected is IConstructible asIConstructible && !trackedConstructibles.Contains(selected as IConstructible))
				{
					trackedConstructibles.Add(asIConstructible);
				}
			}
			CellRectBuilder rectBuilder = new CellRectBuilder();
			HashSet<IConstructible> lastConstructiblesTracked = new HashSet<IConstructible>(trackedConstructibles);
			trackedConstructibles.Clear();
			foreach (IConstructible constructible in lastConstructiblesTracked)
			{
				if (Find.Selector.SelectedObjects.Contains(constructible))
				{
					if (ForbidUtility.IsForbidden(constructible as Thing, Faction.OfPlayer) && !trackForbidden) continue;
					Track(constructible);
					Vector3[] corners = (constructible as Thing).CornerPositions();
					for (int i = 0; i < corners.Length; i++) rectBuilder.ConsiderPoint(corners[i]);
				}
			}
			containingRect = rectBuilder.ToCellRect();
			cacheUpdated = false;
		}
		#endregion track updating

		public void FrameBeingBuilt(Frame frame)
		{
			if (!cachedWorkLeftForFrames.ContainsKey(frame)) return;
			WorkLeft = (WorkLeft - cachedWorkLeftForFrames[frame]) + frame.GetWorkAmount();
			cachedWorkLeftForFrames[frame] = frame.GetWorkAmount();
		}

		public void HighlightTracked(float alpha, float margin)
		{
			if (!highlightRect.CloseEnoughTo(ContainingRect, 0.1f))
			{
				highlightRect = Rect.MinMaxRect(Mathf.Lerp(highlightRect.xMin, ContainingRect.xMin, 0.5f),
					Mathf.Lerp(highlightRect.yMin, ContainingRect.yMin, 0.5f),
					Mathf.Lerp(highlightRect.xMax, ContainingRect.xMax, 0.5f),
					Mathf.Lerp(highlightRect.yMax, ContainingRect.yMax, 0.5f));
			}
			Widgets.DrawBoxSolid(highlightRect.ExpandedBy(margin), new Color(1f, 1f, 1f, alpha));
		}

		public void DebugDraw()
		{
			containingRect.DebugDraw();
			int marginInCells = Mathf.CeilToInt(visibleMargin / Find.CameraDriver.CellSizePixels);
			Find.CameraDriver.CurrentViewRect.ContractedBy(marginInCells).DebugDraw();
		}

		private void Track(IConstructible constructible)
		{
			if (!trackedConstructibles.Contains(constructible))
			{
				trackedConstructibles.Add(constructible);
				if (constructible is Blueprint_Build asBlueprint)
				{
					WorkLeft += asBlueprint.GetWorkAmount();
				}
				if (constructible is Frame asFrame)
				{
					WorkLeft += asFrame.GetWorkAmount();
					cachedWorkLeftForFrames[asFrame] = asFrame.GetWorkAmount();
				}
			}
		}

		private void Untrack(IConstructible constructible)
		{
			if (trackedConstructibles.Contains(constructible))
			{
				trackedConstructibles.Remove(constructible);
				if (constructible is Blueprint_Build asBlueprint)
				{
					WorkLeft -= asBlueprint.GetWorkAmount();
				}
				if (constructible is Frame asFrame)
				{
					if (cachedWorkLeftForFrames.ContainsKey(asFrame))
					{
						WorkLeft -= asFrame.GetWorkAmount();
						cachedWorkLeftForFrames.Remove(asFrame);
					}
					else
					{
						Log.Warning("ConstructibleTotalsTracker is trying to untrack a Frame that has no cached work left value.");
					}
				}
			}
		}
	}
}
