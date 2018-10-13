using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Tiled2Unity
{
	public class TmxData
	{
		public TmxLayer ParentLayer
		{
			get;
			private set;
		}

		public DataEncoding Encoding
		{
			get;
			private set;
		}

		public DataCompression Compression
		{
			get;
			private set;
		}

		public List<TmxChunk> Chunks
		{
			get;
			private set;
		}

		public TmxData(TmxLayer parentLayer)
		{
			ParentLayer = parentLayer;
			Chunks = new List<TmxChunk>();
		}

		public TmxData MakeEmptyCopy(TmxLayer parent)
		{
			TmxData data = new TmxData(parent);
			data.Encoding = Encoding;
			data.Compression = Compression;
			data.Chunks = (from c in Chunks
			select c.MakeEmptyCopy(data)).ToList();
			return data;
		}

		public static TmxData FromDataXml(XElement xml, TmxLayer parentLayer)
		{
			TmxData tmxData = new TmxData(parentLayer);
			tmxData.Encoding = TmxHelper.GetAttributeAsEnum(xml, "encoding", DataEncoding.Xml);
			tmxData.Compression = TmxHelper.GetAttributeAsEnum(xml, "compression", DataCompression.None);
			tmxData.Chunks = TmxChunk.ListFromDataXml(xml, tmxData);
			return tmxData;
		}
	}
}
