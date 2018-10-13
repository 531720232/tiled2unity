using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Tiled2Unity.Geometry
{
	public class PolygonEdgeGroup
	{
		public List<PolygonEdge> PolygonEdges
		{
			get;
			set;
		}

		public void Initialize(List<PointF[]> polygons)
		{
			PolygonEdges = new List<PolygonEdge>();
			int num = 0;
			foreach (PointF[] polygon in polygons)
			{
				CompositionPolygon compositionPolygon = new CompositionPolygon(polygon, num++);
				int num3 = polygon.Length - 1;
				int num4 = 0;
				while (num4 < polygon.Length)
				{
					PointF P = polygon[num3];
					PointF Q = polygon[num4];
					PolygonEdge polygonEdge = PolygonEdges.FirstOrDefault(delegate(PolygonEdge e)
					{
						if (e.P == Q)
						{
							return e.Q == P;
						}
						return false;
					});
					if (polygonEdge != null)
					{
						polygonEdge.AssignMinorPartner(compositionPolygon);
						compositionPolygon.AddEdge(polygonEdge);
					}
					else
					{
						PolygonEdge polygonEdge2 = new PolygonEdge(compositionPolygon, num3);
						compositionPolygon.AddEdge(polygonEdge2);
						PolygonEdges.Add(polygonEdge2);
					}
					num3 = num4++;
				}
			}
		}
	}
}
