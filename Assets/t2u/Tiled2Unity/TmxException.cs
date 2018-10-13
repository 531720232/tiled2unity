using System;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Tiled2Unity
{
	public class TmxException : Exception
	{
		public TmxException(string message)
			: base(message)
		{
		}

		public TmxException(string message, Exception inner)
			: base(message, inner)
		{
		}

		public static void ThrowFormat(string fmt, params object[] args)
		{
			throw new TmxException(string.Format(fmt, args));
		}

		public static void FromAttributeException(Exception inner, XElement element)
		{
			StringBuilder builder = new StringBuilder(inner.Message);
			Array.ForEach(element.Attributes().ToArray(), delegate(XAttribute a)
			{
				builder.AppendFormat("\n  {0}", a.ToString());
			});
			ThrowFormat("Error parsing {0} attributes\n{1}", element.Name, builder.ToString());
		}
	}
}
