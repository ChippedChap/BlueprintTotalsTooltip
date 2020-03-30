using System;
using System.Collections.Generic;
using Verse;
using HarmonyLib;

namespace BlueprintTotalsTooltip.LTChangeNotifiers
{
	[HarmonyPatch(typeof(ListerThings))]
	[HarmonyPatch("Remove")]
	class LTRemoveNotifier
	{
		private static List<Action<Thing>> registeredMethods = new List<Action<Thing>>();

		public static void RegisterMethod(Action<Thing> method) => registeredMethods.Add(method);

		public static void DeregisterMethod(Action<Thing> method)
		{
			if (registeredMethods.Contains(method))
				registeredMethods.Remove(method);
		}

		static void Postfix(Thing t)
		{
			NotifyAll(t);
		}

		private static void NotifyAll(Thing thingRemoved)
		{
			foreach (Action<Thing> method in registeredMethods)
				method(thingRemoved);
		}
	}
}
