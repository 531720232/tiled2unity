using System.Xml.Linq;

namespace Tiled2Unity
{
	public class TmxFrame
	{
		public uint GlobalTileId
		{
			get;
			private set;
		}

		public int DurationMs
		{
			get;
			private set;
		}

		public static TmxFrame FromTileId(uint tileId)
		{
			return new TmxFrame
			{
				GlobalTileId = tileId,
				DurationMs = 0
			};
		}

		public static TmxFrame FromXml(XElement xml, uint globalStartId)
		{
			TmxFrame tmxFrame = new TmxFrame();
			uint attributeAsUInt = TmxHelper.GetAttributeAsUInt(xml, "tileid");
			tmxFrame.GlobalTileId = attributeAsUInt + globalStartId;
			tmxFrame.DurationMs = TmxHelper.GetAttributeAsInt(xml, "duration", 100);
			return tmxFrame;
		}
	}
}
