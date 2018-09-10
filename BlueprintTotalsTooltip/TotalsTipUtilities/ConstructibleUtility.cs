using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using HugsLib;

namespace BlueprintTotalsTooltip.TotalsTipUtilities
{
	static class ConstructibleUtility
	{
		public static List<ThingCount> MaterialsNeededSafe(this IConstructible constructible)
		{
			if (constructible is Blueprint_Install blueprintInstall)
				return new List<ThingCount> { new ThingCount(blueprintInstall.GetInnerIfMinified().def, 1) };
			return new List<ThingCount>(constructible.MaterialsNeeded().Select(x => (ThingCount)x));
		}

		public static ThingCount AddThingDefCounts(ThingCount lhs, ThingCount rhs)
		{
			if (lhs.ThingDef != rhs.ThingDef) Log.Warning("Adding ThingDefCounts with different ThingDefs - New ThingDefCount will have ThingDef of first ThingDefCount");
			return new ThingCount(lhs.ThingDef, lhs.Count + rhs.Count);
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

		public static int GetCountOnMapDifference(this Map map, ThingCount count, bool countForbidden)
		{
			if (count.ThingDef.IsBlueprint) return 0;
			return count.Count - map.GetCountAll(count.ThingDef, countForbidden);
		}

		public static int GetCountInStorageDifference(this Map map, ThingCount count)
		{
			if (count.ThingDef.IsBlueprint) return 0;
			return count.Count - map.resourceCounter.GetCount(count.ThingDef);
		}
	}
}
