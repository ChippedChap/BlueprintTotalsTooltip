using System;
using System.Collections.Generic;

namespace BlueprintTotalsTooltip.SelectorChangeNotifiers
{
	class SelectionChangeNotifierData
	{
		private static List<Action> registeredMethods = new List<Action>();

		public static void RegisterMethod(Action method) => registeredMethods.Add(method);

		public static void DeregisterMethod(Action method)
		{
			if (registeredMethods.Contains(method))
				registeredMethods.Remove(method);
		}

		public static void NotifyChange()
		{
			foreach (Action method in registeredMethods)
				method();
		}
	}
}
