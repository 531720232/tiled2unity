using ClipperLib;
using SD.Tools.Algorithmia.GeneralDataStructures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tiled2Unity
{
	internal class PolylineReduction
	{
		public class InternalPolyline
		{
			public int Id;

			public List<IntPoint> Points = new List<IntPoint>();
		}

		private static int CurrentPolylineId;

		private MultiValueDictionary<IntPoint, InternalPolyline> tablePolyline = new MultiValueDictionary<IntPoint, InternalPolyline>();

		public void AddLine(List<IntPoint> points)
		{
			CurrentPolylineId++;
			points = RemovePointsOnLine(points);
			InternalPolyline internalPolyline = new InternalPolyline();
			internalPolyline.Id = CurrentPolylineId;
			internalPolyline.Points.AddRange(points);
			tablePolyline.Add(internalPolyline.Points.Last(), internalPolyline);
			if (points.First() != points.Last())
			{
				InternalPolyline internalPolyline2 = new InternalPolyline();
				internalPolyline2.Id = CurrentPolylineId;
				internalPolyline2.Points.AddRange(points);
				internalPolyline2.Points.Reverse();
				tablePolyline.Add(internalPolyline2.Points.Last(), internalPolyline2);
			}
		}

		private bool AreNormalsEquivalent(DoublePoint n0, DoublePoint n1)
		{
			double num = Math.Abs(n0.X - n1.X);
			double num2 = Math.Abs(n0.Y - n1.Y);
			if (num < 0.0009765625)
			{
				return num2 < 0.0009765625;
			}
			return false;
		}

		private List<IntPoint> RemovePointsOnLine(List<IntPoint> points)
		{
			int num = 0;
			while (num < points.Count - 2)
			{
				DoublePoint unitNormal = ClipperOffset.GetUnitNormal(points[num], points[num + 1]);
				DoublePoint unitNormal2 = ClipperOffset.GetUnitNormal(points[num], points[num + 2]);
				if (AreNormalsEquivalent(unitNormal, unitNormal2))
				{
					points.RemoveAt(num + 1);
				}
				else
				{
					num++;
				}
			}
			return points;
		}

		private void CombinePolyline(InternalPolyline line0, InternalPolyline line1)
		{
			List<IntPoint> list = new List<IntPoint>();
			list.AddRange(line0.Points);
			line1.Points.Reverse();
			line1.Points.RemoveAt(0);
			list.AddRange(line1.Points);
			AddLine(list);
		}

		private void RemovePolyline(InternalPolyline polyline)
		{
			foreach (InternalPolyline item in (from pairs in tablePolyline
			from line in pairs.Value
			where line.Id == polyline.Id
			select line).ToList())
			{
				tablePolyline.Remove(item.Points.Last(), item);
			}
		}

		public List<List<IntPoint>> Reduce()
		{
			KeyValuePair<IntPoint, HashSet<InternalPolyline>> keyValuePair = tablePolyline.FirstOrDefault((KeyValuePair<IntPoint, HashSet<InternalPolyline>> kvp) => kvp.Value.Count > 1);
			while (keyValuePair.Value != null)
			{
				List<InternalPolyline> list = keyValuePair.Value.ToList();
				InternalPolyline internalPolyline = list[0];
				InternalPolyline internalPolyline2 = list[1];
				RemovePolyline(internalPolyline);
				RemovePolyline(internalPolyline2);
				CombinePolyline(internalPolyline, internalPolyline2);
				keyValuePair = tablePolyline.FirstOrDefault((KeyValuePair<IntPoint, HashSet<InternalPolyline>> kvp) => kvp.Value.Count > 1);
			}
			return (from pairs in tablePolyline
			from line in pairs.Value
			select line into ln
			group ln by ln.Id into grp
			select grp.First() into l
			select l.Points).ToList();
		}
	}
}
