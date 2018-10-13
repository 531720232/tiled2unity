using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Tiled2Unity
{
	public class TmxObjectType
	{
		public string Name
		{
			get;
			private set;
		}

		public Color Color
		{
			get;
			private set;
		}

		public Dictionary<string, TmxObjectTypeProperty> Properties
		{
			get;
			private set;
		}

		public TmxObjectType()
		{
			Name = "";
			Color =new Color32(128, 128, 128,255);
			Properties = new Dictionary<string, TmxObjectTypeProperty>();
		}

		public static TmxObjectType FromXml(XElement xml)
		{
			return new TmxObjectType
			{
				Name = TmxHelper.GetAttributeAsString(xml, "name", ""),
				Color = TmxHelper.GetAttributeAsColor(xml, "color", new Color32(128, 128, 128, 255)),
				Properties = TmxObjectTypeProperty.FromObjectTypeXml(xml)
			};
		}
	}
}
