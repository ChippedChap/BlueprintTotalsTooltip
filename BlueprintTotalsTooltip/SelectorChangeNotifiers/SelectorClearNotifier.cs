using RimWorld;
using Harmony;

namespace BlueprintTotalsTooltip.SelectorChangeNotifiers
{
	[HarmonyPatch(typeof(Selector))]
	[HarmonyPatch("ClearSelection")]
	class SelectorClearNotifier
	{
		static void Postfix()
		{
			SelectionChangeNotifierData.NotifyChange();
		}
	}
}
