using System.Collections.Generic;
using System.Drawing;

namespace Tiled2Unity.Geometry
{
	public class CompositionPolygon
	{
		public int InitialId
		{
			get;
			private set;
		}

		public List<PointF> Points
		{
			get;
			private set;
		}

		public List<PolygonEdge> Edges
		{
			get;
			private set;
		}

		public CompositionPolygon(IEnumerable<PointF> points, int initialId)
		{
			InitialId = initialId;
			Points = new List<PointF>();
			Edges = new List<PolygonEdge>();
			Points.AddRange(points);
		}

		public void AddEdge(PolygonEdge edge)
		{
			Edges.Add(edge);
		}

		public int NextIndex(int index)
		{
			return (index + 1) % Points.Count;
		}

		public int PrevIndex(int index)
		{
			if (index == 0)
			{
				return Points.Count - 1;
			}
			return (index - 1) % Points.Count;
		}

		public PointF NextPoint(int index)
		{
			index = NextIndex(index);
			return Points[index];
		}

		public PointF PrevPoint(int index)
		{
			index = PrevIndex(index);
			return Points[index];
		}

		public void AbsorbPolygon(int q, CompositionPolygon minor, int pMinor)
		{
			int num = minor.Points.Count - 2;
			List<PointF> list = new List<PointF>();
			for (int i = 0; i < num; i++)
			{
				int index = (pMinor + 1 + i) % minor.Points.Count;
				list.Add(minor.Points[index]);
			}
			Points.InsertRange(q, list);
			foreach (PolygonEdge edge in minor.Edges)
			{
				if (!Edges.Contains(edge))
				{
					Edges.Add(edge);
				}
			}
		}

		public void ReplaceEdgesWithPolygon(CompositionPolygon replacement, PolygonEdge ignoreEdge)
		{
			foreach (PolygonEdge edge in Edges)
			{
				if (edge != ignoreEdge)
				{
					if (edge.MajorPartner == this)
					{
						edge.ReplaceMajor(replacement);
					}
					else if (edge.MinorPartner == this)
					{
						edge.ReplaceMinor(replacement);
					}
				}
			}
		}

		public void UpdateEdgeIndices(PolygonEdge ignoreEdge)
		{
			foreach (PolygonEdge edge in Edges)
			{
				if (edge != ignoreEdge)
				{
					edge.UpdateIndices(this);
				}
			}
		}
	}
}
