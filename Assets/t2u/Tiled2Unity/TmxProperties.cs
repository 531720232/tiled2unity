using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Tiled2Unity
{
	public class TmxProperties
	{
		public IDictionary<string, TmxProperty> PropertyMap
		{
			get;
			private set;
		}

		public TmxProperties()
		{
			PropertyMap = new Dictionary<string, TmxProperty>(StringComparer.InvariantCultureIgnoreCase);
		}

		public string GetPropertyValueAsString(string name)
		{
			return PropertyMap[name].Value;
		}

		public string GetPropertyValueAsString(string name, string defaultValue)
		{
			if (PropertyMap.ContainsKey(name))
			{
				return PropertyMap[name].Value;
			}
			return defaultValue;
		}

		public int GetPropertyValueAsInt(string name)
		{
			try
			{
				return Convert.ToInt32(PropertyMap[name].Value);
			}
			catch (FormatException inner)
			{
				throw new TmxException(string.Format("Error evaulating property '{0}={1}'\n  '{1}' is not an integer", name, PropertyMap[name].Value), inner);
			}
		}

		public int GetPropertyValueAsInt(string name, int defaultValue)
		{
			if (PropertyMap.ContainsKey(name))
			{
				return GetPropertyValueAsInt(name);
			}
			return defaultValue;
		}

		public bool GetPropertyValueAsBoolean(string name)
		{
			bool result = false;
			try
			{
				result = Convert.ToBoolean(PropertyMap[name].Value);
				return result;
			}
			catch (FormatException)
			{
				Logger.WriteWarning("Property '{0}' value '{1}' cannot be converted to a boolean.", name, PropertyMap[name].Value);
				return result;
			}
		}

		public bool GetPropertyValueAsBoolean(string name, bool defaultValue)
		{
			if (PropertyMap.ContainsKey(name))
			{
				return GetPropertyValueAsBoolean(name);
			}
			return defaultValue;
		}

		public T GetPropertyValueAsEnum<T>(string name)
		{
			return TmxHelper.GetStringAsEnum<T>(PropertyMap[name].Value);
		}

		public T GetPropertyValueAsEnum<T>(string name, T defaultValue)
		{
			if (PropertyMap.ContainsKey(name))
			{
				return GetPropertyValueAsEnum<T>(name);
			}
			return defaultValue;
		}

		public static TmxProperties FromXml(XElement elem)
		{
			TmxProperties tmxProperties = new TmxProperties();
			var enumerable = from elem1 in elem.Elements("properties")
			from elem2 in elem1.Elements("property")
			select new
			{
				Name = TmxHelper.GetAttributeAsString(elem2, "name"),
				Type = TmxHelper.GetAttributeAsEnum(elem2, "type", TmxPropertyType.String),
				Value = (TmxHelper.GetAttributeAsString(elem2, "value", null) ?? elem2.Value)
			};
			if (enumerable.Count() > 0)
			{
				Logger.WriteVerbose("Parse properties ...");
			}
			foreach (var item in enumerable)
			{
				tmxProperties.PropertyMap[item.Name] = new TmxProperty
				{
					Name = item.Name,
					Type = item.Type,
					Value = item.Value
				};
			}
			return tmxProperties;
		}
	}
}
