using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace Tiled2Unity
{
	public class TmxObjectPolyline : TmxObject, TmxHasPoints
	{
		public List<PointF> Points
		{
			get;
			set;
		}

		public TmxObjectPolyline()
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
            Rect result = new Rect(num, num3, num2 - num, num4 - num3);
			result.position+=new Vector2 (base.Position.X,Position.Y);
			return result;
		}

		protected override void InternalFromXml(XElement xml, TmxMap tmxMap)
		{
			IEnumerable<PointF> source = from pt in xml.Element("polyline").Attribute("points").Value.Split(' ')
			let x = float.Parse(pt.Split(',')[0])
			let y = float.Parse(pt.Split(',')[1])
			select new PointF(x, y);
			if (source.Count() == 2)
			{
				PointF pointF = source.First();
				PointF pointF2 = source.Last();
				PointF item = TmxMath.MidPoint(pointF, pointF2);
				Points = new List<PointF>
				{
					pointF,
					item,
					pointF2
				};
			}
			else
			{
				Points = source.ToList();
			}
		}

		protected override string InternalGetDefaultName()
		{
			return "PolylineObject";
		}

		public bool ArePointsClosed()
		{
			return false;
		}
	}
}
