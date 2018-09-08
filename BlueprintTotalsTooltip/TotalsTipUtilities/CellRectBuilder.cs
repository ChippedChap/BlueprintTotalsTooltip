using UnityEngine;
using Verse;

namespace BlueprintTotalsTooltip.TotalsTipUtilities
{
	class CellRectBuilder
	{
		private bool firstPoint = true;
		private float maxX = 0;
		private float minX = 0;
		private float maxZ = 0;
		private float minZ = 0;

		public CellRect ToCellRect()
		{
			CellRect cellRect = CellRect.Empty;
			cellRect.maxX = (int)maxX - 1;
			cellRect.minX = (int)minX;
			cellRect.maxZ = (int)maxZ - 1;
			cellRect.minZ = (int)minZ;
			return cellRect;
		}

		public void ConsiderPoint(Vector3 point)
		{
			if (InitialPoint(point)) return;
			maxX = Mathf.Max(maxX, point.x);
			minX = Mathf.Min(minX, point.x);
			maxZ = Mathf.Max(maxZ, point.z);
			minZ = Mathf.Min(minZ, point.z);
		}

		private bool InitialPoint(Vector3 point)
		{
			if (firstPoint)
			{
				maxX = minX = point.x;
				minZ = maxZ = point.z;
				firstPoint = false;
				return true;
			}
			return false;
		}
	}
}
