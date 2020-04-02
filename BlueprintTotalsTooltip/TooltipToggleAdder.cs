using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;
using BlueprintTotalsTooltip.ChangeDetection;
using System.Collections.Generic;
using System;

namespace BlueprintTotalsTooltip
{
	[HarmonyPatch(typeof(PlaySettings))]
	[HarmonyPatch("DoPlaySettingsGlobalControls")]
	class TooltipToggleAdder
	{
		private static List<Action> methodsToNotifyOnToggle = new List<Action>();

		public static void RegisterMethod(Action method) => methodsToNotifyOnToggle.Add(method);

		public static void DeregisterMethod(Action method)
		{
			if (methodsToNotifyOnToggle.Contains(method))
				methodsToNotifyOnToggle.Remove(method);
		}

		static void Postfix(WidgetRow row, bool worldView)
		{
			if (worldView || row == null) return;
			bool current = TotalsTooltipMod.ShouldDrawTooltip;
			bool previous = current;
			row.ToggleableIcon(ref current, AssetLoader.totalsTooltipToggleTexture, "ShowTotalsTooltipTip".Translate(TotalsTooltipMod.toggleTipDraw.MainKeyLabel), SoundDefOf.Mouseover_ButtonToggle);
			if (previous != current)
			{
				TotalsTooltipMod.ShouldDrawTooltip.Value = current;
				NotifyPlaySettingToggled();
			}
		}

		public static void NotifyPlaySettingToggled()
		{
			TotalsTooltipMod.ShouldDrawTooltip.ForceSaveChanges();
			foreach (Action method in methodsToNotifyOnToggle)
				method();
		}

		// IN MEMORIAM: My first working transpiler patch previously written in PlaySettingChangeDetector.cs
		// Superseded by additional code in this class. ~9/8/2018 to 4/1/2019
	}
}
