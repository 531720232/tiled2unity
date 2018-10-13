using System.Drawing;

namespace Tiled2Unity.Geometry
{
	public class PolygonEdge
	{
		public bool HasBeenMerged
		{
			get;
			private set;
		}

		public PointF P
		{
			get;
			private set;
		}

		public PointF Q
		{
			get;
			private set;
		}

		public float Length2
		{
			get;
			private set;
		}

		public CompositionPolygon MajorPartner
		{
			get;
			private set;
		}

		public int MajorPartner_pIndex
		{
			get;
			private set;
		}

		public int MajorPartner_qIndex
		{
			get;
			private set;
		}

		public CompositionPolygon MinorPartner
		{
			get;
			private set;
		}

		public int MinorPartner_pIndex
		{
			get;
			private set;
		}

		public int MinorPartner_qIndex
		{
			get;
			private set;
		}

		public PolygonEdge(CompositionPolygon compPolygon, int p)
		{
			HasBeenMerged = false;
			int num = (p + 1) % compPolygon.Points.Count;
			P = compPolygon.Points[p];
			Q = compPolygon.Points[num];
			MajorPartner = compPolygon;
			MajorPartner_pIndex = p;
			MajorPartner_qIndex = num;
			float num2 = P.X - Q.X;
			float num3 = P.Y - Q.Y;
			float num4 = num2;
			float num5 = num4 * num4;
			float num6 = num3;
			Length2 = num5 + num6 * num6;
		}

		public void AssignMinorPartner(CompositionPolygon polygon)
		{
			ReplaceMinor(polygon);
		}

		public void ReplaceMajor(CompositionPolygon polygon)
		{
			MajorPartner = polygon;
			MajorPartner_pIndex = MajorPartner.Points.IndexOf(P);
			MajorPartner_qIndex = MajorPartner.Points.IndexOf(Q);
		}

		public void ReplaceMinor(CompositionPolygon polygon)
		{
			MinorPartner = polygon;
			MinorPartner_pIndex = MinorPartner.Points.IndexOf(P);
			MinorPartner_qIndex = MinorPartner.Points.IndexOf(Q);
		}

		public bool CanMergePolygons()
		{
			PointF a = MajorPartner.PrevPoint(MajorPartner_pIndex);
			PointF b = MajorPartner.Points[MajorPartner_pIndex];
			PointF c = MinorPartner.NextPoint(MinorPartner_pIndex);
			if (Math.Cross(a, b, c) > 0f)
			{
				return false;
			}
			PointF a2 = MajorPartner.NextPoint(MajorPartner_qIndex);
			PointF b2 = MajorPartner.Points[MajorPartner_qIndex];
			PointF c2 = MinorPartner.PrevPoint(MinorPartner_qIndex);
			if (Math.Cross(a2, b2, c2) < 0f)
			{
				return false;
			}
			return true;
		}

		public void MergePolygons()
		{
			MajorPartner.AbsorbPolygon(MajorPartner_qIndex, MinorPartner, MinorPartner_pIndex);
			MinorPartner.ReplaceEdgesWithPolygon(MajorPartner, this);
			MajorPartner.UpdateEdgeIndices(this);
			HasBeenMerged = true;
		}

		public void UpdateIndices(CompositionPolygon polygon)
		{
			if (polygon == MajorPartner)
			{
				MajorPartner_pIndex = polygon.Points.IndexOf(P);
				MajorPartner_qIndex = polygon.Points.IndexOf(Q);
			}
			else if (polygon == MinorPartner)
			{
				MinorPartner_pIndex = polygon.Points.IndexOf(P);
				MinorPartner_qIndex = polygon.Points.IndexOf(Q);
			}
		}
	}
}
