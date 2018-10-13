using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Tiled2Unity.Geometry
{
	public class ComposeConvexPolygons
	{
		public PolygonEdgeGroup PolygonEdgeGroup
		{
			get;
			private set;
		}

		public List<PointF[]> ConvexPolygons
		{
			get;
			private set;
		}

		public ComposeConvexPolygons()
		{
			PolygonEdgeGroup = new PolygonEdgeGroup();
		}

		public List<PointF[]> Compose(List<PointF[]> triangles)
		{
			PolygonEdgeGroup.Initialize(triangles);
			CombinePolygons();
			return ConvexPolygons;
		}

		private void CombinePolygons()
		{
			List<CompositionPolygon> list = new List<CompositionPolygon>();
			foreach (PolygonEdge polygonEdge in PolygonEdgeGroup.PolygonEdges)
			{
				if (polygonEdge.MajorPartner != null)
				{
					list.Add(polygonEdge.MajorPartner);
				}
				if (polygonEdge.MinorPartner != null)
				{
					list.Add(polygonEdge.MinorPartner);
				}
			}
			list = list.Distinct().ToList();
			PolygonEdgeGroup.PolygonEdges.RemoveAll(delegate(PolygonEdge e)
			{
				if (e.MinorPartner != null)
				{
					return e.MajorPartner == null;
				}
				return true;
			});
			foreach (PolygonEdge item in from edge in PolygonEdgeGroup.PolygonEdges
			orderby edge.Length2 descending
			select edge)
			{
				if (item.CanMergePolygons())
				{
					list.Remove(item.MinorPartner);
					item.MergePolygons();
				}
			}
			ConvexPolygons = (from cp in list
			select cp.Points.ToArray()).ToList();
		}
	}
}
