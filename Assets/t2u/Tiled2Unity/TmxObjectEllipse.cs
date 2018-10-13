using System.Drawing;
using System.Xml.Linq;
using UnityEngine;

namespace Tiled2Unity
{
	public class TmxObjectEllipse : TmxObject
	{
		public float Radius => base.Size.Width * 0.5f;

		public bool IsCircle()
		{
			return base.Size.Width == base.Size.Height;
		}

		public override Rect GetWorldBounds()
		{
			return new Rect(base.Position.X,Position.Y, base.Size.Width,Size.Height);
		}

		protected override void InternalFromXml(XElement xml, TmxMap tmxMap)
		{
		}

		protected override string InternalGetDefaultName()
		{
			if (IsCircle())
			{
				return "CircleObject";
			}
			return "EllipseObject";
		}
	}
}
