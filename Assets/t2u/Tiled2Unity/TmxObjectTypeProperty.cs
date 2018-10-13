using System.Collections.Generic;
using System.Xml.Linq;

namespace Tiled2Unity
{
	public class TmxObjectTypeProperty
	{
		public string Name
		{
			get;
			private set;
		}

		public TmxPropertyType Type
		{
			get;
			private set;
		}

		public string Default
		{
			get;
			set;
		}

		public static Dictionary<string, TmxObjectTypeProperty> FromObjectTypeXml(XElement xmlObjectType)
		{
			Dictionary<string, TmxObjectTypeProperty> dictionary = new Dictionary<string, TmxObjectTypeProperty>();
			foreach (XElement item in xmlObjectType.Elements("property"))
			{
				TmxObjectTypeProperty tmxObjectTypeProperty = new TmxObjectTypeProperty();
				tmxObjectTypeProperty.Name = TmxHelper.GetAttributeAsString(item, "name", "");
				tmxObjectTypeProperty.Type = TmxHelper.GetAttributeAsEnum(item, "type", TmxPropertyType.String);
				tmxObjectTypeProperty.Default = TmxHelper.GetAttributeAsString(item, "default", "");
				dictionary.Add(tmxObjectTypeProperty.Name, tmxObjectTypeProperty);
			}
			return dictionary;
		}
	}
}
