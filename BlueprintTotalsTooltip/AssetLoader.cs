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
		public static readonly Texture2D selectionTrackingIndicator = new Texture2D(64, 64);
		public static readonly List<Texture2D> highlightTextures = new List<Texture2D>();

		private static readonly int highlightTexNum = 25;
		private static readonly float highlightAlphaStep = 0.01f;

		static AssetLoader()
		{
			LoadHighlightTextures();
		}

		public static Texture2D GetPreloadedTexOfOpacity(float opacity)
		{
			int index = Mathf.Clamp((int)(opacity / highlightAlphaStep), 0, highlightTexNum + 1);
			return highlightTextures[index];
		}

		private static void LoadHighlightTextures()
		{
			for (int i = 0; i < highlightTexNum + 1; i++)
			{
				highlightTextures.Add(new Texture2D(1, 1));
				highlightTextures[i].SetPixel(0, 0, new Color(1f, 1f, 1f, highlightAlphaStep * i));
				highlightTextures[i].Apply();
			}
		}
	}	
}
