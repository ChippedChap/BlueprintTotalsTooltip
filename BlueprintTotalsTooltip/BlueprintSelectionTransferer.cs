using HarmonyLib;
using Verse;
using RimWorld;

namespace BlueprintTotalsTooltip
{
	[HarmonyPatch(typeof(Blueprint_Build))]
	[HarmonyPatch("MakeSolidThing")]
	class BlueprintSelectionTransferer
	{
		public static bool transferring = true;

		static void Postfix(Blueprint_Build __instance, Thing __result)
		{
			if (transferring && Find.Selector.SelectedObjects.Contains(__instance) && __result != null)
			{
				Find.Selector.SelectedObjects.Add(__result);
				SelectionDrawer.Notify_Selected(__result);
				SelectorChangeNotifiers.SelectionChangeNotifierData.NotifyChange();
			}
		}
	}
}
