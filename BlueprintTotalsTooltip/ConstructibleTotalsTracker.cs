using BlueprintTotalsTooltip.TotalsTipUtilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RimWorld;
using Verse;

namespace BlueprintTotalsTooltip
{
	class ConstructibleTotalsTracker
	{
		#region settings
		public float visibleMargin;
		#endregion settings

		private HashSet<IConstructible> trackedConstructibles = new HashSet<IConstructible>();
		private List<ThingDefCount> cachedCosts;
		private bool cacheUpdated = false;

		private CellRect containingRect = CellRect.Empty;
		private Rect highlightRect = Rect.zero;

		public int NumberTracked => trackedConstructibles.Count;

		public Rect ContainingRect
		{
			get
			{
				if (containingRect.Equals(CellRect.Empty))
				{
					if (Find.Selector.NumSelected == 0)
						TrackVisibleConstructibles();
					else
						TrackSelectedConstructibles();
				}
				return containingRect.ToScreenRect();
			}
		}

		public void ClearTracked() => trackedConstructibles.Clear();

		public void ResolveSettings(TotalsTooltipMod mod)
		{
			visibleMargin = mod.VisibilityMargin;
		}

		#region track updating
		public void TryTrackConstructible(Thing constructible)
		{
			if (trackedConstructibles.Contains(constructible as IConstructible) ||
				!constructible.ThingIsVisible(visibleMargin, out Vector3[] corners) &&
				!Find.Selector.SelectedObjects.Contains(constructible)) return;
			trackedConstructibles.Add(constructible as IConstructible);
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
				trackedConstructibles.Remove(constructible as IConstructible);
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
			HashSet<IConstructible> oldSet = trackedConstructibles;
			trackedConstructibles = BuildVisibleSetAndRect();
			cacheUpdated = false;
		}

		public void TrackSelectedConstructibles()
		{
			foreach (object selected in Find.Selector.SelectedObjects)
				if (selected is IConstructible && !trackedConstructibles.Contains(selected as IConstructible))
					trackedConstructibles.Add(selected as IConstructible);
			CellRectBuilder rectBuilder = new CellRectBuilder();
			foreach (IConstructible constructible in new HashSet<IConstructible>(trackedConstructibles))
			{
				if (!Find.Selector.SelectedObjects.Contains(constructible as IConstructible))
				{
					trackedConstructibles.Remove(constructible);
					continue;
				}
				Vector3[] corners = (constructible as Thing).CornerPositions();
				for (int i = 0; i < corners.Length; i++)
					rectBuilder.ConsiderPoint(corners[i]);
			}
			containingRect = rectBuilder.ToCellRect();
			cacheUpdated = false;
		}
		#endregion track updating

		public List<ThingDefCount> GetTrackedTotals()
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

		public void HighlightTracked(Texture2D highlightTex, float margin)
		{
			if (!highlightRect.CloseEnoughTo(ContainingRect, 0.1f))
			{
				highlightRect = Rect.MinMaxRect(Mathf.Lerp(highlightRect.xMin, ContainingRect.xMin, 0.5f),
					Mathf.Lerp(highlightRect.yMin, ContainingRect.yMin, 0.5f),
					Mathf.Lerp(highlightRect.xMax, ContainingRect.xMax, 0.5f),
					Mathf.Lerp(highlightRect.yMax, ContainingRect.yMax, 0.5f));
			}
			GUI.DrawTexture(highlightRect.ExpandedBy(margin), highlightTex);
		}

		public void DebugDraw()
		{
			containingRect.DebugDraw();
			int marginInCells = Mathf.CeilToInt(visibleMargin / Find.CameraDriver.CellSizePixels);
			Find.CameraDriver.CurrentViewRect.ContractedBy(marginInCells).DebugDraw();
		}

		private HashSet<IConstructible> BuildVisibleSetAndRect()
		{
			List<Thing> candidateThings = Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.Construction);
			HashSet<IConstructible> visibleConstructibles = new HashSet<IConstructible>();
			CellRectBuilder crBuilder = new CellRectBuilder();
			for (int i = 0; i < candidateThings.Count; i++)
			{
				Thing candidateThing = candidateThings[i];
				if (!candidateThing.ThingIsVisible(visibleMargin, out Vector3[] corners)) continue;
				visibleConstructibles.Add(candidateThing as IConstructible);
				for (int j = 0; j < corners.Length; j++) crBuilder.ConsiderPoint(corners[j]);
			}
			containingRect = crBuilder.ToCellRect();
			return visibleConstructibles;
		}
	}
}
