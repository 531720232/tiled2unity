using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using System.Xml.Linq;

namespace Tiled2Unity
{
	public class TmxObjectRectangle : TmxObjectPolygon
	{
		protected override void InternalFromXml(XElement xml, TmxMap tmxMap)
		{
			base.Points = new List<PointF>();
			base.Points.Add(new PointF(0f, 0f));
			base.Points.Add(new PointF(base.Size.Width, 0f));
			base.Points.Add(new PointF(base.Size.Width, base.Size.Height));
			base.Points.Add(new PointF(0f, base.Size.Height));
			if (base.Size.Width == 0f || base.Size.Height == 0f)
			{
				Logger.WriteWarning("Warning: Rectangle has zero width or height. Line {0}\n\t{1}", ((IXmlLineInfo)xml).LineNumber, xml.ToString());
			}
		}

		protected override string InternalGetDefaultName()
		{
			return "RectangleObject";
		}
	}
}
