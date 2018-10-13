using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Tiled2Unity
{
	public class TmxObjectGroup : TmxLayerNode
	{
		public List<TmxObject> Objects
		{
			get;
			private set;
		}

		public Color Color
		{
			get;
			private set;
		}

		public TmxObjectGroup(TmxLayerNode parent, TmxMap tmxMap)
			: base(parent, tmxMap)
		{
			Objects = new List<TmxObject>();
		}

		public Rect GetWorldBounds(PointF translation)
		{
            Rect rectangleF = default(Rect);
			foreach (TmxObject @object in Objects)
			{
                Rect worldBounds = @object.GetWorldBounds();
				worldBounds.position+=new Vector2(translation.X,translation.Y);
                rectangleF = RectEx.Union(rectangleF, worldBounds);

            }
			return rectangleF;
		}

		public Rect GetWorldBounds()
		{
			return GetWorldBounds(new PointF(0f, 0f));
		}

		public override string ToString()
		{
			return $"{{ ObjectGroup name={base.Name}, numObjects={Objects.Count()} }}";
		}

		public override void Visit(ITmxVisitor visitor)
		{
			visitor.VisitObjectLayer(this);
			foreach (TmxObject @object in Objects)
			{
				visitor.VisitObject(@object);
			}
		}

		public static TmxObjectGroup FromXml(XElement xml, TmxLayerNode parent, TmxMap tmxMap)
		{
			TmxObjectGroup tmxObjectGroup = new TmxObjectGroup(parent, tmxMap);
			tmxObjectGroup.FromXmlInternal(xml);
			tmxObjectGroup.Color = TmxHelper.GetAttributeAsColor(xml, "color", new Color32(128, 128, 128,255));
			Logger.WriteVerbose("Parsing objects in object group '{0}'", tmxObjectGroup.Name);
			IEnumerable<TmxObject> source = from obj in xml.Elements("object")
			select TmxObject.FromXml(obj, tmxObjectGroup, tmxMap);
			tmxObjectGroup.Objects = (from o in source
			orderby TmxMath.ObjectPointFToMapSpace(tmxMap, o.Position).Y
			select o).ToList();
			return tmxObjectGroup;
		}
	}
}
