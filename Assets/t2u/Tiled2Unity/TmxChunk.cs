using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Tiled2Unity
{
	public class TmxChunk
	{
		public TmxData ParentData
		{
			get;
			private set;
		}

		public int X
		{
			get;
			set;
		}

		public int Y
		{
			get;
			set;
		}

		public int Width
		{
			get;
			set;
		}

		public int Height
		{
			get;
			set;
		}

		public List<uint> TileIds
		{
			get;
			private set;
		}

		public TmxChunk(TmxData parentData)
		{
			ParentData = parentData;
			TileIds = new List<uint>();
		}

		public TmxChunk MakeEmptyCopy(TmxData parentData)
		{
			return new TmxChunk(parentData)
			{
				X = X,
				Y = Y,
				Width = Width,
				Height = Height,
				TileIds = Enumerable.Repeat(0u, TileIds.Count).ToList()
			};
		}

		public Point GetXYIndices(int index)
		{
			return new Point(index % Width, index / Width);
		}

		public int GetTileIndex(int x, int y)
		{
			return y * Width + x;
		}

		public uint GetRawTileIdAt(int x, int y)
		{
			int tileIndex = GetTileIndex(x, y);
			return TileIds[tileIndex];
		}

		public uint GetTileIdAt(int x, int y)
		{
			return TmxMath.GetTileIdWithoutFlags(GetRawTileIdAt(x, y));
		}

		public static List<TmxChunk> ListFromDataXml(XElement xml, TmxData parentData)
		{
			if (xml.Element("chunk") == null)
			{
				return ListFromDataXml_Finite(xml, parentData);
			}
			return ListFromDataXml_Infinite(xml, parentData);
		}

		private static List<TmxChunk> ListFromDataXml_Finite(XElement xml, TmxData parentData)
		{
			TmxChunk tmxChunk = new TmxChunk(parentData);
			tmxChunk.X = 0;
			tmxChunk.Y = 0;
			tmxChunk.Width = parentData.ParentLayer.Width;
			tmxChunk.Height = parentData.ParentLayer.Height;
			tmxChunk.ReadTileIds(xml);
			return tmxChunk.ToEnumerable().ToList();
		}

		private static List<TmxChunk> ListFromDataXml_Infinite(XElement xml, TmxData parentData)
		{
			List<TmxChunk> list = new List<TmxChunk>();
			foreach (XElement item in xml.Elements("chunk"))
			{
				TmxChunk tmxChunk = new TmxChunk(parentData);
				tmxChunk.X = TmxHelper.GetAttributeAsInt(item, "x", 0);
				tmxChunk.Y = TmxHelper.GetAttributeAsInt(item, "y", 0);
				tmxChunk.Width = TmxHelper.GetAttributeAsInt(item, "width", 0);
				tmxChunk.Height = TmxHelper.GetAttributeAsInt(item, "height", 0);
				tmxChunk.ReadTileIds(item);
				list.Add(tmxChunk);
			}
			return list;
		}

		private void ReadTileIds(XElement xml)
		{
			TileIds.Clear();
			if (ParentData.Encoding == DataEncoding.Xml)
			{
				ReadTileIds_Xml(xml);
			}
			else if (ParentData.Encoding == DataEncoding.Csv)
			{
				ReadTiledIds_Csv(xml.Value);
			}
			else if (ParentData.Encoding == DataEncoding.Base64)
			{
				ReadTileIds_Base64(xml.Value);
			}
			else
			{
				TmxException.ThrowFormat("Unsupported encoding for chunk data in layer '{0}'", ParentData.ParentLayer);
			}
			for (int i = 0; i < TileIds.Count; i++)
			{
				uint tileId = TileIds[i];
				tileId = TmxMath.GetTileIdWithoutFlags(tileId);
				if (!ParentData.ParentLayer.ParentMap.Tiles.ContainsKey(tileId))
				{
					TileIds[i] = 0u;
				}
			}
		}

		private void ReadTileIds_Xml(XElement xml)
		{
			Logger.WriteVerbose("Parsing layer chunk data as Xml elements ...");
			IEnumerable<uint> source = from t in xml.Elements("tile")
			select TmxHelper.GetAttributeAsUInt(t, "gid", 0u);
			TileIds = source.ToList();
		}

		private void ReadTiledIds_Csv(string data)
		{
			Logger.WriteVerbose("Parsing layer data as CSV ...");
			StringReader stringReader = new StringReader(data);
			string empty = string.Empty;
			do
			{
				empty = stringReader.ReadLine();
				if (!string.IsNullOrEmpty(empty))
				{
					IEnumerable<uint> collection = from val in empty.Split(',')
					where !string.IsNullOrEmpty(val)
					select Convert.ToUInt32(val);
					TileIds.AddRange(collection);
				}
			}
			while (empty != null);
		}

		private void ReadTileIds_Base64(string data)
		{
			if (ParentData.Compression == DataCompression.None)
			{
				Logger.WriteVerbose("Parsing layer chunk data as base64 string ...");
				TileIds = data.Base64ToBytes().ToUInts().ToList();
			}
			else if (ParentData.Compression == DataCompression.Gzip)
			{
				Logger.WriteVerbose("Parsing layer chunk data as gzip compressed string ...");
				TileIds = data.Base64ToBytes().GzipDecompress().ToUInts()
					.ToList();
			}
			else if (ParentData.Compression == DataCompression.Zlib)
			{
				Logger.WriteVerbose("Parsing layer chunk data as zlib string ...");
				TileIds = data.Base64ToBytes().ZlibDeflate().ToUInts()
					.ToList();
			}
			else
			{
				TmxException.ThrowFormat("Unsupported compression for chunk data in layer '{0}'", ParentData.ParentLayer);
			}
		}
	}
}
