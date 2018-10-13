using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Tiled2Unity
{
	public class TmxObjectTypes
	{
		public Dictionary<string, TmxObjectType> TmxObjectTypeMapping
		{
			get;
			private set;
		}

		public TmxObjectTypes()
		{
			TmxObjectTypeMapping = new Dictionary<string, TmxObjectType>(StringComparer.InvariantCultureIgnoreCase);
		}

		public TmxObjectType GetValueOrDefault(string key)
		{
			if (TmxObjectTypeMapping.ContainsKey(key))
			{
				return TmxObjectTypeMapping[key];
			}
			return new TmxObjectType();
		}

		public TmxObjectType GetValueOrNull(string key)
		{
			if (key != null && TmxObjectTypeMapping.ContainsKey(key))
			{
				return TmxObjectTypeMapping[key];
			}
			return null;
		}

		public static TmxObjectTypes FromXmlFile(string xmlPath)
		{
			TmxObjectTypes tmxObjectTypes = new TmxObjectTypes();
			foreach (XElement item in XDocument.Load(xmlPath).Element("objecttypes").Elements("objecttype"))
			{
				TmxObjectType tmxObjectType = TmxObjectType.FromXml(item);
				tmxObjectTypes.TmxObjectTypeMapping[tmxObjectType.Name] = tmxObjectType;
			}
			return tmxObjectTypes;
		}
	}
}
