using System.Collections.Generic;
using System.Xml.Linq;

namespace Tiled2Unity
{
	public class TmxAnimation
	{
		public List<TmxFrame> Frames
		{
			get;
			private set;
		}

		public int TotalTimeMs
		{
			get;
			private set;
		}

		public TmxAnimation()
		{
			Frames = new List<TmxFrame>();
		}

		public static TmxAnimation FromXml(XElement xml, uint globalStartId)
		{
			TmxAnimation tmxAnimation = new TmxAnimation();
			foreach (XElement item in xml.Elements("frame"))
			{
				TmxFrame tmxFrame = TmxFrame.FromXml(item, globalStartId);
				tmxAnimation.Frames.Add(tmxFrame);
				tmxAnimation.TotalTimeMs += tmxFrame.DurationMs;
			}
			return tmxAnimation;
		}

		public static TmxAnimation FromTileId(uint globalTileId)
		{
			TmxAnimation tmxAnimation = new TmxAnimation();
			TmxFrame item = TmxFrame.FromTileId(globalTileId);
			tmxAnimation.Frames.Add(item);
			return tmxAnimation;
		}
	}
}
