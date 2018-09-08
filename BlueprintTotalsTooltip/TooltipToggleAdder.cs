using Harmony;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;
using BlueprintTotalsTooltip.ChangeDetection;

namespace BlueprintTotalsTooltip
{
	[HarmonyPatch(typeof(PlaySettings))]
	[HarmonyPatch("DoPlaySettingsGlobalControls")]
	class TooltipToggleAdder
	{
		private static KeyBindingDef toggleTipDraw = DefDatabase<KeyBindingDef>.GetNamed("ToggleTracking");

		static void Postfix(WidgetRow row, bool worldView)
		{
			if (worldView || row == null) return;
			row.ToggleableIcon(ref TotalsTooltipDrawer.shouldDraw, AssetLoader.totalsTooltipToggleTexture, "ShowTotalsTooltipTip".Translate(new object[] { toggleTipDraw.MainKeyLabel }), SoundDefOf.Mouseover_ButtonToggle);
			CheckDrawSettingToggle();
		}

		static void CheckDrawSettingToggle()
		{
			if (toggleTipDraw.KeyDownEvent)
			{
				TotalsTooltipDrawer.shouldDraw = !TotalsTooltipDrawer.shouldDraw;
				PlaySettingsChangeDetector.NotifyAll();
				if (TotalsTooltipDrawer.shouldDraw)
					SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
				else
					SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
			}
		}
	}
}
