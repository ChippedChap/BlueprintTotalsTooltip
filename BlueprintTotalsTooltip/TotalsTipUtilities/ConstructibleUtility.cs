using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using HugsLib;
using UnityEngine;

namespace BlueprintTotalsTooltip.TotalsTipUtilities
{
	static class ConstructibleUtility
	{
		public static List<ThingDefCount> MaterialsNeededSafe(this IConstructible constructible)
		{
			if (constructible is Blueprint_Install blueprintInstall)
				return new List<ThingDefCount> { new ThingDefCount(blueprintInstall.GetInnerIfMinified().def, 1) };
			return new List<ThingDefCount>(constructible.MaterialsNeeded().Select(x => (ThingDefCount)x));
		}

		public static ThingDefCount AddThingDefCounts(ThingDefCount lhs, ThingDefCount rhs)
		{
			if (lhs.ThingDef != rhs.ThingDef) Log.Warning("Adding ThingDefCounts with different ThingDefs - New ThingDefCount will have ThingDef of first ThingDefCount");
			return new ThingDefCount(lhs.ThingDef, lhs.Count + rhs.Count);
		}

		public static int GetCountAll(this Map map, ThingDef def, bool countForbidden)
		{
			List<Thing> thingList = map.listerThings.ThingsOfDef(def);
			int count = 0;
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing innerIfMinified = thingList[i].GetInnerIfMinified();
				if (innerIfMinified.def.CountAsResource && !thingList[i].IsNotFresh() && (!thingList[i].IsForbidden(Faction.OfPlayer) || countForbidden))
					count += innerIfMinified.stackCount;
			}
			return count;
		}

		public static int GetCountOnMapDifference(this Map map, ThingDefCount count, bool countForbidden)
		{
			if (count.ThingDef.IsBlueprint) return 0;
			return count.Count - map.GetCountAll(count.ThingDef, countForbidden);
		}

		public static int GetCountInStorageDifference(this Map map, ThingDefCount count)
		{
			if (count.ThingDef.IsBlueprint) return 0;
			return count.Count - map.resourceCounter.GetCount(count.ThingDef);
		}

		public static int GetWorkAmount(this Blueprint_Build blueprint)
		{
			return Mathf.CeilToInt(blueprint.def.entityDefToBuild.GetStatValueAbstract(StatDefOf.WorkToBuild, blueprint.stuffToUse)/60f);
		}

		public static int GetWorkAmount(this Frame frame)
		{
			return Mathf.CeilToInt(frame.WorkLeft/60f);
		}
	}
}
