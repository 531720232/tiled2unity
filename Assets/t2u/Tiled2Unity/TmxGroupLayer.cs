using System.Xml.Linq;

namespace Tiled2Unity
{
	public class TmxGroupLayer : TmxLayerNode
	{
		public TmxGroupLayer(TmxLayerNode parent, TmxMap tmxMap)
			: base(parent, tmxMap)
		{
		}

		public override void Visit(ITmxVisitor visitor)
		{
			visitor.VisitGroupLayer(this);
			foreach (TmxLayerNode layerNode in base.LayerNodes)
			{
				layerNode.Visit(visitor);
			}
		}

		public static TmxGroupLayer FromXml(XElement xml, TmxLayerNode parent, TmxMap tmxMap)
		{
			TmxGroupLayer tmxGroupLayer = new TmxGroupLayer(parent, tmxMap);
			tmxGroupLayer.FromXmlInternal(xml);
			return tmxGroupLayer;
		}
	}
}
