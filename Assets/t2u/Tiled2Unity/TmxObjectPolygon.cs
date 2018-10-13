using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace Tiled2Unity
{
	public class TmxObjectPolygon : TmxObject, TmxHasPoints
	{
		public List<PointF> Points
		{
			get;
			set;
		}

		public TmxObjectPolygon()
		{
			Points = new List<PointF>();
		}

		public override Rect GetWorldBounds()
		{
			float num = 3.40282347E+38f;
			float num2 = -3.40282347E+38f;
			float num3 = 3.40282347E+38f;
			float num4 = -3.40282347E+38f;
			foreach (PointF point in Points)
			{
				num = Math.Min(num, point.X);
				num2 = Math.Max(num2, point.X);
				num3 = Math.Min(num3, point.Y);
				num4 = Math.Max(num4, point.Y);
			}
			var result = new Rect(num, num3, num2 - num, num4 - num3);
			result.position+=new Vector2(base.Position.X,base.Position.Y);
			return result;
		}

		protected override void InternalFromXml(XElement xml, TmxMap tmxMap)
		{
			IEnumerable<PointF> source = from pt in xml.Element("polygon").Attribute("points").Value.Split(' ')
			let x = float.Parse(pt.Split(',')[0])
			let y = float.Parse(pt.Split(',')[1])
			select new PointF(x, y);
			Points = source.ToList();
			float num = 0f;
			for (int i = 1; i < Points.Count(); i++)
			{
				PointF pointF = Points[i - 1];
				PointF pointF2 = Points[i];
				float num2 = (pointF2.X - pointF.X) * (0f - (pointF2.Y + pointF.Y));
				num += num2;
			}
			if (num < 0f)
			{
				Points.Reverse();
			}
		}

		protected override string InternalGetDefaultName()
		{
			return "PolygonObject";
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (Points == null)
			{
				stringBuilder.Append("<empty>");
			}
			else
			{
				foreach (PointF point in Points)
				{
					stringBuilder.AppendFormat("({0}, {1})", point.X, point.Y);
					if (point != Points.Last())
					{ 
                    
						stringBuilder.AppendFormat(", ");
					}
				}
			}
			return $"{GetType().Name} {GetNonEmptyName()} {base.Position} points=({stringBuilder.ToString()})";
		}

		public bool ArePointsClosed()
		{
			return true;
		}

		public static TmxObjectPolygon FromRectangle(TmxMap tmxMap, TmxObjectRectangle tmxRectangle)
		{
			TmxObjectPolygon tmxObjectPolygon = new TmxObjectPolygon();
			TmxObject.CopyBaseProperties(tmxRectangle, tmxObjectPolygon);
			tmxObjectPolygon.Points = tmxRectangle.Points;
			return tmxObjectPolygon;
		}
	}
}
