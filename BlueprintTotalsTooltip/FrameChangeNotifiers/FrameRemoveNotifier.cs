using HarmonyLib;
using Verse;
using RimWorld;

namespace BlueprintTotalsTooltip.FrameChangeNotifiers
{
	[HarmonyPatch(typeof(ThingOwner))]
	[HarmonyPatch("NotifyRemoved")]
	class FrameRemoveNotifier
	{
		static void Postfix(ThingOwner<Thing> __instance)
		{
			if (__instance.Owner is Frame)
				FrameChangeNotifierData.NotifyChange();
		}
	}
}
