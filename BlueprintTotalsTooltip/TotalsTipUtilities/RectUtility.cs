using UnityEngine;
using Verse;

namespace BlueprintTotalsTooltip.TotalsTipUtilities
{
	static class RectUtility
	{
		public static bool ThingIsVisible(this Thing thing, float visibiltyMargin, out Vector3[] corners)
		{
			int marginInCells = Mathf.CeilToInt(visibiltyMargin / Find.CameraDriver.CellSizePixels);
			CellRect visibleRect = Find.CameraDriver.CurrentViewRect.ContractedBy(marginInCells);
			corners = thing.CornerPositions();
			for (int i = 0; i < corners.Length; i++)
				if (!visibleRect.Contains(corners[i])) return false;
			return true;
		}

		public static Vector3[] CornerPositions(this Thing thing)
		{
			float xOffset = thing.RotatedSize.x / 2f;
			float zOffset = thing.RotatedSize.z / 2f;
			Vector3[] corners = new Vector3[4];
			corners[0] = new Vector3(thing.DrawPos.x - xOffset, 0, thing.DrawPos.z - zOffset);
			corners[1] = new Vector3(thing.DrawPos.x - xOffset, 0, thing.DrawPos.z + zOffset);
			corners[2] = new Vector3(thing.DrawPos.x + xOffset, 0, thing.DrawPos.z + zOffset);
			corners[3] = new Vector3(thing.DrawPos.x + xOffset, 0, thing.DrawPos.z - zOffset);
			return corners;
		}

		public static bool Contains(this CellRect cellRect, Vector3 point)
		{
			bool xValid = cellRect.minX <= point.x && point.x <= cellRect.maxX + 1f;
			bool zValid = cellRect.minZ <= point.z && point.z <= cellRect.maxZ + 1f;
			return xValid && zValid;
		}

		public static CellRect ExpandToContain(this CellRect rect, Vector3 point)
		{
			int maxX = (int)Mathf.Max(rect.maxX, point.x - 1);
			int minX = (int)Mathf.Min(rect.minX, point.x);
			int maxZ = (int)Mathf.Max(rect.maxZ, point.z - 1);
			int minZ = (int)Mathf.Min(rect.minZ, point.z);
			return CellRect.FromLimits(minX, minZ, maxX, maxZ);
		}

		public static Rect ToScreenRect(this CellRect cellRect)
		{
			Vector2 max = UI.MapToUIPosition(new Vector3(cellRect.maxX + 1f, 0f, cellRect.maxZ + 1f));
			Vector2 min = UI.MapToUIPosition(new Vector3(cellRect.minX, 0f, cellRect.minZ));
			return Rect.MinMaxRect(min.x, max.y, max.x, min.y); ;
		}

		public static Rect ClampRectInRect(this Rect rect, Rect containingRect)
		{
			float newX = rect.x;
			float newY = rect.y;
			if (containingRect.xMax < rect.x + rect.width)
				newX = containingRect.xMax - rect.width;
			if (rect.x < containingRect.xMin)
				newX = containingRect.xMin;
			if (containingRect.yMax < rect.y + rect.height)
				newY = containingRect.yMax - rect.height;
			if (rect.y < containingRect.yMin)
				newY = containingRect.yMin;
			return new Rect(newX, newY, rect.width, rect.height);
		}

		public static bool CloseEnoughTo(this Rect lhs, Rect rhs, float threshold)
		{
			return Mathf.Abs(lhs.xMax - rhs.xMax) <= threshold &&
				Mathf.Abs(lhs.yMax - rhs.yMax) <= threshold &&
				Mathf.Abs(lhs.xMin - rhs.xMin) <= threshold &&
				Mathf.Abs(lhs.yMin - rhs.yMin) <= threshold;
		}

		public static Rect WidthContractedBy(this Rect original, float xMargin)
		{
			return new Rect(original.x + xMargin, original.y, original.width - xMargin * 2, original.height);
		}

		public static void DrawBracketsAroundRect(Rect rectToDrawIn)
		{
			Texture2D bracketTexture = AssetLoader.bracketTexture;
			GUI.DrawTexture(rectToDrawIn, bracketTexture);
			Rect bottomRight = new Rect(rectToDrawIn.x + rectToDrawIn.width, rectToDrawIn.y, -rectToDrawIn.width, rectToDrawIn.height);
			GUI.DrawTexture(bottomRight, bracketTexture);
			Rect topLeft = new Rect(rectToDrawIn.x, rectToDrawIn.y + rectToDrawIn.height, rectToDrawIn.width, -rectToDrawIn.height);
			GUI.DrawTexture(topLeft, bracketTexture);
			Rect topRight = new Rect(bottomRight.x, topLeft.y, -rectToDrawIn.width, -rectToDrawIn.height);
			GUI.DrawTexture(topRight, bracketTexture);
		}
	}
}
