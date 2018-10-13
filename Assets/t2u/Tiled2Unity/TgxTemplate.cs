using System.Xml.Linq;

namespace Tiled2Unity
{
	public class TgxTemplate
	{
		public TmxMap TemplateGroupMap
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			private set;
		}

		public uint LocalId
		{
			get;
			private set;
		}

		public uint GlobalId
		{
			get;
			private set;
		}

		public XElement ObjectXml
		{
			get;
			private set;
		}

		public TgxTemplate(TmxMap templateGroupMap)
		{
			TemplateGroupMap = templateGroupMap;
		}

		public XElement Templatize(XElement xml)
		{
			foreach (XAttribute item in ObjectXml.Attributes())
			{
				if (xml.Attribute(item.Name) == null)
				{
					xml.SetAttributeValue(item.Name, item.Value);
				}
			}
			foreach (XElement item2 in ObjectXml.Elements())
			{
				if (xml.Element(item2.Name) != null)
				{
					xml.SetElementValue(item2.Name, item2.Value);
				}
			}
			return xml;
		}

		public static TgxTemplate FromXml(XElement xml, uint firstId, TmxMap map)
		{
			TgxTemplate tgxTemplate = new TgxTemplate(map);
			tgxTemplate.Name = TmxHelper.GetAttributeAsString(xml, "name");
			tgxTemplate.LocalId = TmxHelper.GetAttributeAsUInt(xml, "id");
			tgxTemplate.GlobalId = firstId + tgxTemplate.LocalId;
			tgxTemplate.ObjectXml = xml.Element("object");
			return tgxTemplate;
		}
	}
}
