using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;

namespace Tiled2Unity
{
	public abstract class TmxLayerNode : TmxHasProperties, ITmxVisit
	{
		public enum IgnoreSettings
		{
			False,
			True,
			Collision,
			Visual
		}

		public TmxLayerNode ParentNode
		{
			get;
			private set;
		}

		public TmxMap ParentMap
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			protected set;
		}

		public bool Visible
		{
			get;
			protected set;
		}

		public float Opacity
		{
			get;
			protected set;
		}

		public PointF Offset
		{
			get;
			protected set;
		}

		public IgnoreSettings Ignore
		{
			get;
			protected set;
		}

		public TmxProperties Properties
		{
			get;
			protected set;
		}

		public string ExplicitSortingLayerName
		{
			get;
			set;
		}

		public int? ExplicitSortingOrder
		{
			get;
			set;
		}

		public int DrawOrderIndex
		{
			get;
			set;
		}

		public int DepthBufferIndex
		{
			get;
			set;
		}

		public string UnityLayerOverrideName
		{
			get;
			protected set;
		}

		public List<TmxLayerNode> LayerNodes
		{
			get;
			protected set;
		}

		public TmxLayerNode(TmxLayerNode parent, TmxMap tmxMap)
		{
			DrawOrderIndex = -1;
			DepthBufferIndex = -1;
			ParentNode = parent;
			ParentMap = tmxMap;
			LayerNodes = new List<TmxLayerNode>();
		}

		public PointF GetCombinedOffset()
		{
			PointF pointF = Offset;
			for (TmxLayerNode parentNode = ParentNode; parentNode != null; parentNode = parentNode.ParentNode)
			{
				pointF = TmxMath.AddPoints(pointF, parentNode.Offset);
			}
			return pointF;
		}

		public string GetSortingLayerName()
		{
			if (!string.IsNullOrEmpty(ExplicitSortingLayerName))
			{
				return ExplicitSortingLayerName;
			}
			if (ParentNode != null)
			{
				return ParentNode.GetSortingLayerName();
			}
			return ParentMap.ExplicitSortingLayerName;
		}

		public int GetSortingOrder()
		{
			if (ExplicitSortingOrder.HasValue)
			{
				return ExplicitSortingOrder.Value;
			}
			return DrawOrderIndex;
		}

		public float GetRecursiveOpacity()
		{
			float num = (ParentNode != null) ? ParentNode.GetRecursiveOpacity() : 1f;
			return Opacity * num;
		}

		public abstract void Visit(ITmxVisitor visitor);

		public static List<TmxLayerNode> ListFromXml(XElement xmlRoot, TmxLayerNode parent, TmxMap tmxMap)
		{
			List<TmxLayerNode> list = new List<TmxLayerNode>();
			foreach (XElement item in xmlRoot.Elements())
			{
				TmxLayerNode tmxLayerNode = null;
				if (item.Name == (XName)"layer" || item.Name == (XName)"imagelayer")
				{
					tmxLayerNode = TmxLayer.FromXml(item, parent, tmxMap);
				}
				else if (item.Name == (XName)"objectgroup")
				{
					tmxLayerNode = TmxObjectGroup.FromXml(item, parent, tmxMap);
				}
				else if (item.Name == (XName)"group")
				{
					tmxLayerNode = TmxGroupLayer.FromXml(item, parent, tmxMap);
				}
				if (tmxLayerNode != null && tmxLayerNode.Visible && tmxLayerNode.Ignore != IgnoreSettings.True)
				{
					list.Add(tmxLayerNode);
				}
			}
			return list;
		}

		protected void FromXmlInternal(XElement xml)
		{
			Name = TmxHelper.GetAttributeAsString(xml, "name", "");
			Visible = (TmxHelper.GetAttributeAsInt(xml, "visible", 1) == 1);
			Opacity = TmxHelper.GetAttributeAsFloat(xml, "opacity", 1f);
			PointF offset = new PointF(0f, 0f);
			offset.X = TmxHelper.GetAttributeAsFloat(xml, "offsetx", 0f);
			offset.Y = TmxHelper.GetAttributeAsFloat(xml, "offsety", 0f);
			Offset = offset;
			Properties = TmxProperties.FromXml(xml);
			Ignore = Properties.GetPropertyValueAsEnum("unity:ignore", IgnoreSettings.False);
			ExplicitSortingLayerName = Properties.GetPropertyValueAsString("unity:sortingLayerName", "");
			if (Properties.PropertyMap.ContainsKey("unity:sortingOrder"))
			{
				ExplicitSortingOrder = Properties.GetPropertyValueAsInt("unity:sortingOrder");
			}
			UnityLayerOverrideName = Properties.GetPropertyValueAsString("unity:layer", "");
			LayerNodes = ListFromXml(xml, this, ParentMap);
		}
	}
}
