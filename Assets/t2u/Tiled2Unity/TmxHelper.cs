using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Tiled2Unity
{
	public class TmxHelper
	{
		public static string GetAttributeAsString(XElement elem, string attrName)
		{
			return elem.Attribute(attrName).Value;
		}

		public static string GetAttributeAsString(XElement elem, string attrName, string defaultValue)
		{
			if (elem.Attribute(attrName) == null)
			{
				return defaultValue;
			}
			return GetAttributeAsString(elem, attrName);
		}

		public static int GetAttributeAsInt(XElement elem, string attrName)
		{
			return Convert.ToInt32(elem.Attribute(attrName).Value);
		}

		public static int GetAttributeAsInt(XElement elem, string attrName, int defaultValue)
		{
			if (elem.Attribute(attrName) == null)
			{
				return defaultValue;
			}
			return GetAttributeAsInt(elem, attrName);
		}

		public static uint GetAttributeAsUInt(XElement elem, string attrName)
		{
			return Convert.ToUInt32(elem.Attribute(attrName).Value);
		}

		public static uint GetAttributeAsUInt(XElement elem, string attrName, uint defaultValue)
		{
			if (elem.Attribute(attrName) == null)
			{
				return defaultValue;
			}
			return GetAttributeAsUInt(elem, attrName);
		}

		public static float GetAttributeAsFloat(XElement elem, string attrName)
		{
			return Convert.ToSingle(elem.Attribute(attrName).Value);
		}

		public static float GetAttributeAsFloat(XElement elem, string attrName, float defaultValue)
		{
			if (elem.Attribute(attrName) == null)
			{
				return defaultValue;
			}
			return GetAttributeAsFloat(elem, attrName);
		}

		public static string GetAttributeAsFullPath(XElement elem, string attrName)
		{
			return Path.GetFullPath(elem.Attribute(attrName).Value);
		}

		public static Color GetAttributeAsColor(XElement elem, string attrName)
		{
			return ColorFromHtml(elem.Attribute(attrName).Value);
		}

		public static Color GetAttributeAsColor(XElement elem, string attrName, Color defaultValue)
		{
			if (elem.Attribute(attrName) == null)
			{
				return defaultValue;
			}
			return GetAttributeAsColor(elem, attrName);
		}

		public static T GetStringAsEnum<T>(string enumString)
		{
			enumString = enumString.Replace("-", "_");
			T result = default(T);
			try
			{
				result = (T)Enum.Parse(typeof(T), enumString, true);
				return result;
			}
			catch
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendFormat("Could not convert '{0}' to enum of type '{1}'\n", enumString, typeof(T).ToString());
				stringBuilder.AppendFormat("Choices are:\n");
				foreach (T value in Enum.GetValues(typeof(T)))
				{
					stringBuilder.AppendFormat("  {0}\n", value.ToString());
				}
				TmxException.ThrowFormat(stringBuilder.ToString());
				return result;
			}
		}

		public static T GetAttributeAsEnum<T>(XElement elem, string attrName)
		{
			return GetStringAsEnum<T>(elem.Attribute(attrName).Value.Replace("-", "_"));
		}

		public static T GetAttributeAsEnum<T>(XElement elem, string attrName, T defaultValue)
		{
			if (elem.Attribute(attrName) == null)
			{
				return defaultValue;
			}
			return GetAttributeAsEnum<T>(elem, attrName);
		}

		public static TmxProperties GetPropertiesWithTypeDefaults(TmxHasProperties hasProperties, TmxObjectTypes objectTypes)
		{
			TmxProperties tmxProperties = new TmxProperties();
			string key = null;
			if (hasProperties is TmxObject)
			{
				key = (hasProperties as TmxObject).Type;
			}
			TmxObjectType valueOrNull = objectTypes.GetValueOrNull(key);
			if (valueOrNull != null)
			{
				foreach (TmxObjectTypeProperty value in valueOrNull.Properties.Values)
				{
					tmxProperties.PropertyMap[value.Name] = new TmxProperty
					{
						Name = value.Name,
						Type = value.Type,
						Value = value.Default
					};
				}
			}
			foreach (TmxProperty value2 in hasProperties.Properties.PropertyMap.Values)
			{
				tmxProperties.PropertyMap[value2.Name] = value2;
			}
			return tmxProperties;
		}

		public static Color ColorFromHtml(string html)
		{
			html = html.TrimStart('#');
			html = html.PadLeft(6, '0');
			html = html.PadLeft(8, 'F');
			try
			{
                var a1 = Convert.ToInt32(html, 16);
              //  Convert.ToByte(a1);
                return new Color(a1&255,a1>>8&255,a1>>16&255);
			}
			catch
			{
				return new Color(255, 0, 255);
			}
		}
	}
}
