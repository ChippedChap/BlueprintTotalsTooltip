using RimWorld;
using Verse;
using Harmony;

namespace BlueprintTotalsTooltip.SelectorChangeNotifiers
{
	[HarmonyPatch(typeof(Selector))]
	[HarmonyPatch("Deselect")]
	class SelectorDeselectNotifier
	{
		public static void Prefix(out int __state)
		{
			__state = Find.Selector.NumSelected;
		}

		public static void Postfix(int __state)
		{
			if (__state != Find.Selector.NumSelected)
				SelectionChangeNotifierData.NotifyChange();
		}
	}
}
