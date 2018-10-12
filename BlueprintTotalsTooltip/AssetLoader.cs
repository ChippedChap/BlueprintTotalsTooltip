using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace BlueprintTotalsTooltip
{
	[StaticConstructorOnStartup]
	static class AssetLoader
	{
		public static readonly Texture2D totalsTooltipToggleTexture = ContentFinder<Texture2D>.Get("ShowTotalsTooltip");
		public static readonly Texture2D workLeftTexture = ContentFinder<Texture2D>.Get("UI/Buttons/AutoRebuild");
		public static readonly Texture2D bracketTexture = ContentFinder<Texture2D>.Get("UI/Overlays/SelectionBracket");
	}	
}
