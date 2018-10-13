using System.Drawing;
using System.Xml.Linq;
using UnityEngine;

namespace Tiled2Unity
{
	public abstract class TmxObject : TmxHasProperties
	{
		public int Id
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			private set;
		}

		public string Type
		{
			get;
			private set;
		}

		public bool Visible
		{
			get;
			private set;
		}

		public PointF Position
		{
			get;
			private set;
		}

		public SizeF Size
		{
			get;
			private set;
		}

		public float Rotation
		{
			get;
			private set;
		}

		public TmxProperties Properties
		{
			get;
			private set;
		}

		public TmxObjectGroup ParentObjectGroup
		{
			get;
			private set;
		}

		public string GetNonEmptyName()
		{
			if (string.IsNullOrEmpty(Name))
			{
				return InternalGetDefaultName();
			}
			return Name;
		}

		public override string ToString()
		{
			return $"{GetType().Name} {GetNonEmptyName()} pos={Position}, size={Size} rot = {Rotation}";
		}

		public void BakeRotation()
		{
			PointF[] array = new PointF[1]
			{
				PointF.Empty
			};
			TmxMath.RotatePoints(array, this);
			float x = Position.X - array[0].X;
			float y = Position.Y - array[0].Y;
			Position = new PointF(x, y);
			Position = TmxMath.Sanitize(Position);
			Rotation = 0f;
		}

		protected static void CopyBaseProperties(TmxObject from, TmxObject to)
		{
			to.Id = from.Id;
			to.Name = from.Name;
			to.Type = from.Type;
			to.Visible = from.Visible;
			to.Position = from.Position;
			to.Size = from.Size;
			to.Rotation = from.Rotation;
			to.Properties = from.Properties;
			to.ParentObjectGroup = from.ParentObjectGroup;
		}

		public Rect GetOffsetWorldBounds()
		{
			Rect worldBounds = GetWorldBounds();
			PointF combinedOffset = ParentObjectGroup.GetCombinedOffset();
			worldBounds.position+=new Vector2(combinedOffset.X,combinedOffset.Y);
			return worldBounds;
		}

		public abstract Rect GetWorldBounds();

		protected abstract void InternalFromXml(XElement xml, TmxMap tmxMap);

		protected abstract string InternalGetDefaultName();

		public static TmxObject FromXml(XElement xml, TmxObjectGroup tmxObjectGroup, TmxMap tmxMap)
		{
			Logger.WriteVerbose("Parsing object ...");
			uint attributeAsUInt = TmxHelper.GetAttributeAsUInt(xml, "tid", 0u);
			if (attributeAsUInt != 0 && tmxMap.Templates.TryGetValue(attributeAsUInt, out TgxTemplate value))
			{
				xml = value.Templatize(xml);
				tmxMap = value.TemplateGroupMap;
			}
			TmxObject tmxObject = null;
			if (xml.Element("ellipse") != null)
			{
				tmxObject = new TmxObjectEllipse();
			}
			else if (xml.Element("polygon") != null)
			{
				tmxObject = new TmxObjectPolygon();
			}
			else if (xml.Element("polyline") != null)
			{
				tmxObject = new TmxObjectPolyline();
			}
			else if (xml.Attribute("gid") != null)
			{
				uint attributeAsUInt2 = TmxHelper.GetAttributeAsUInt(xml, "gid");
				attributeAsUInt2 = TmxMath.GetTileIdWithoutFlags(attributeAsUInt2);
				if (tmxMap.Tiles.ContainsKey(attributeAsUInt2))
				{
					tmxObject = new TmxObjectTile();
				}
				else
				{
					Logger.WriteWarning("Tile Id {0} not found in tilesets. Using a rectangle instead.\n{1}", attributeAsUInt2, xml.ToString());
					tmxObject = new TmxObjectRectangle();
				}
			}
			else
			{
				tmxObject = new TmxObjectRectangle();
			}
			tmxObject.Id = TmxHelper.GetAttributeAsInt(xml, "id", 0);
			tmxObject.Name = TmxHelper.GetAttributeAsString(xml, "name", "");
			tmxObject.Type = TmxHelper.GetAttributeAsString(xml, "type", "");
			tmxObject.Visible = (TmxHelper.GetAttributeAsInt(xml, "visible", 1) == 1);
			tmxObject.ParentObjectGroup = tmxObjectGroup;
			float attributeAsFloat = TmxHelper.GetAttributeAsFloat(xml, "x");
			float attributeAsFloat2 = TmxHelper.GetAttributeAsFloat(xml, "y");
			float attributeAsFloat3 = TmxHelper.GetAttributeAsFloat(xml, "width", 0f);
			float attributeAsFloat4 = TmxHelper.GetAttributeAsFloat(xml, "height", 0f);
			float attributeAsFloat5 = TmxHelper.GetAttributeAsFloat(xml, "rotation", 0f);
			tmxObject.Position = new PointF(attributeAsFloat, attributeAsFloat2);
			tmxObject.Size = new SizeF(attributeAsFloat3, attributeAsFloat4);
			tmxObject.Rotation = attributeAsFloat5;
			tmxObject.Properties = TmxProperties.FromXml(xml);
			tmxObject.InternalFromXml(xml, tmxMap);
			return tmxObject;
		}
	}
}
