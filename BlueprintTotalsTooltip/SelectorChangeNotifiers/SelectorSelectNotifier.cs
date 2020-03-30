using RimWorld;
using Verse;
using HarmonyLib;

namespace BlueprintTotalsTooltip.SelectorChangeNotifiers
{
	[HarmonyPatch(typeof(Selector))]
	[HarmonyPatch("Select")]
	class SelectorSelectNotifier
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
