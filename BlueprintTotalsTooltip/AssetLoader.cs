using UnityEngine;
using Verse;
using HugsLib;
using HugsLib.Settings;

namespace BlueprintTotalsTooltip
{
	[StaticConstructorOnStartup]
	static class AssetLoader
	{
		public static Texture2D trackedHighlightTexture = SolidColorMaterials.NewSolidColorTexture(1f, 1f, 1f, 0.1f);
		public static readonly Texture2D totalsTooltipToggleTexture = ContentFinder<Texture2D>.Get("ShowTotalsTooltip");

		static AssetLoader()
		{
			ModSettingsPack modSettings = HugsLibController.SettingsManager.GetModSettings("BlueprintTotalsTooltip");
			float alpha = float.Parse(modSettings.PeekValue("highlightOpacity") ?? "0.1");
			trackedHighlightTexture.SetPixel(0, 0, new Color(1f, 1f, 1f, alpha));
			trackedHighlightTexture.Apply();
		}
	}
}
