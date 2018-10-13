using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Tiled2Unity
{
	public class TgxTemplateGroup
	{
		public TmxMap ParentMap
		{
			get;
			private set;
		}

		public TmxMap TemplateMap
		{
			get;
			private set;
		}

		public uint FirstTemplateId
		{
			get;
			private set;
		}

		public string Source
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			private set;
		}

		public uint NextTemplateId
		{
			get;
			private set;
		}

		public List<TsxTileset> Tilesets
		{
			get;
			private set;
		}

		public List<TgxTemplate> Templates
		{
			get;
			private set;
		}

		public TgxTemplateGroup(TmxMap map)
		{
			ParentMap = map;
			Tilesets = new List<TsxTileset>();
			Templates = new List<TgxTemplate>();
		}

		public static TgxTemplateGroup FromXml(XElement xml, TmxMap map)
		{
			TgxTemplateGroup tgxTemplateGroup = new TgxTemplateGroup(map);
			tgxTemplateGroup.FirstTemplateId = TmxHelper.GetAttributeAsUInt(xml, "firsttid");
			tgxTemplateGroup.Source = Path.GetFullPath(TmxHelper.GetAttributeAsString(xml, "source"));
			if (File.Exists(tgxTemplateGroup.Source))
			{
				using (new ChDir(tgxTemplateGroup.Source))
				{
					XDocument xDocument = TmxMap.LoadDocument(tgxTemplateGroup.Source);
					tgxTemplateGroup.ParseTemplateGroupXml(xDocument.Root);
				}
			}
			else
			{
				Logger.WriteError("Template group file does not exist: {0}", tgxTemplateGroup.Source);
			}
			tgxTemplateGroup.Templates.ForEach(delegate(TgxTemplate t)
			{
				map.Templates.Add(t.GlobalId, t);
			});
			return tgxTemplateGroup;
		}

		public void ParseTemplateGroupXml(XElement xml)
		{
			Name = TmxHelper.GetAttributeAsString(xml, "name");
			NextTemplateId = TmxHelper.GetAttributeAsUInt(xml, "nexttemplateid");
			TemplateMap = ParentMap.MakeTemplate(Name);
			foreach (XElement item in xml.Descendants("tileset"))
			{
				TsxTileset.FromXml(item, TemplateMap);
			}
			foreach (XElement item2 in xml.Descendants("template"))
			{
				TgxTemplate tgxTemplate = TgxTemplate.FromXml(item2, FirstTemplateId, TemplateMap);
				if (tgxTemplate != null)
				{
					Templates.Add(tgxTemplate);
				}
			}
		}
	}
}
