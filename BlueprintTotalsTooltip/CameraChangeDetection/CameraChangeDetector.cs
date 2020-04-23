using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;

namespace BlueprintTotalsTooltip.CameraChangeDetection
{
	class CameraChangeDetector
	{
		List<Action> registeredMethods = new List<Action>();
		CellRect lastViewRect = CellRect.Empty;

		public void RegisterMethod(Action method) => registeredMethods.Add(method);

		public void DeregisterMethod(Action method)
		{
			if (registeredMethods.Contains(method))
				registeredMethods.Remove(method);
		}

		public void OnGUI()
		{
			if (lastViewRect != Find.CameraDriver.CurrentViewRect)
				NotifyAll();
			lastViewRect = Find.CameraDriver.CurrentViewRect;
		}

		private void NotifyAll()
		{
			foreach (Action method in registeredMethods)
				method();
		}
	}
}
