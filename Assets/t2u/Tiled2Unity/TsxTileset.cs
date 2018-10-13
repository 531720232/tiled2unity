using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Tiled2Unity
{
	public class TsxTileset
	{
		public TmxMap TmxMap
		{
			get;
			private set;
		}

		public uint FirstGlobalId
		{
			get;
			private set;
		}

		public string Source
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			private set;
		}

		public int TileWidth
		{
			get;
			private set;
		}

		public int TileHeight
		{
			get;
			private set;
		}

		public int Spacing
		{
			get;
			private set;
		}

		public int Margin
		{
			get;
			private set;
		}

		public int TileCount
		{
			get;
			private set;
		}

		public int Columns
		{
			get;
			private set;
		}

		public PointF TileOffset
		{
			get;
			private set;
		}

		public TmxImage Image
		{
			get;
			private set;
		}

		public List<TmxTile> Tiles
		{
			get;
			private set;
		}

		public TmxProperties Properties
		{
			get;
			private set;
		}

		public TsxTileset(TmxMap tmxMap)
		{
			TmxMap = tmxMap;
			Tiles = new List<TmxTile>();
			Properties = new TmxProperties();
		}

		public static TsxTileset FromXml(XElement xml, TmxMap tmxMap)
		{
			TsxTileset tsxTileset = new TsxTileset(tmxMap);
			tsxTileset.FirstGlobalId = TmxHelper.GetAttributeAsUInt(xml, "firstgid");
			XAttribute xAttribute = xml.Attribute("source");
			if (xAttribute == null)
			{
				ParseTilesetXml(xml, tsxTileset);
			}
			else
			{
				ParseTilesetSource(xAttribute.Value, tsxTileset);
			}
			tsxTileset.Tiles.ForEach(delegate(TmxTile t)
			{
				tmxMap.Tiles.Add(t.GlobalId, t);
			});
			return tsxTileset;
		}

		public static TsxTileset FromImageLayerXml(XElement xml, TmxMap tmxMap)
		{
			TsxTileset tsxTileset = new TsxTileset(tmxMap);
			tsxTileset.Name = TmxHelper.GetAttributeAsString(xml, "name");
			XElement xElement = xml.Element("image");
			if (xElement == null)
			{
				Logger.WriteWarning("Image Layer '{0}' has no image assigned.", tsxTileset.Name);
				return null;
			}
			TmxProperties tmxProperties = TmxProperties.FromXml(xml);
			string propertyValueAsString = tmxProperties.GetPropertyValueAsString("unity:namePrefix", "");
			string propertyValueAsString2 = tmxProperties.GetPropertyValueAsString("unity:namePostfix", "");
			TmxImage tmxImage = TmxImage.FromXml(xElement, propertyValueAsString, propertyValueAsString2);
			uint num = 1u;
			if (tmxMap.Tiles.Count > 0)
			{
				num = tmxMap.Tiles.Max((KeyValuePair<uint, TmxTile> t) => t.Key) + 1;
			}
			uint num2 = 1u;
			uint globalId = num + num2;
			TmxTile tmxTile = new TmxTile(tmxMap, globalId, num2, tsxTileset.Name, tmxImage);
			tmxTile.SetTileSize(tmxImage.Size.Width, tmxImage.Size.Height);
			tmxTile.SetLocationOnSource(0, 0);
			tsxTileset.Tiles.Add(tmxTile);
			tsxTileset.Tiles.ForEach(delegate(TmxTile t)
			{
				tmxMap.Tiles.Add(t.GlobalId, t);
			});
			return tsxTileset;
		}

		private static void ParseTilesetXml(XElement xml, TsxTileset tileset)
		{
			tileset.Name = TmxHelper.GetAttributeAsString(xml, "name");
			Logger.WriteVerbose("Parse internal tileset '{0}' (gid = {1}) ...", tileset.Name, tileset.FirstGlobalId);
			tileset.TileWidth = TmxHelper.GetAttributeAsInt(xml, "tilewidth");
			tileset.TileHeight = TmxHelper.GetAttributeAsInt(xml, "tileheight");
			tileset.Spacing = TmxHelper.GetAttributeAsInt(xml, "spacing", 0);
			tileset.Margin = TmxHelper.GetAttributeAsInt(xml, "margin", 0);
			PointF empty = PointF.Empty;
			XElement xElement = xml.Element("tileoffset");
			if (xElement != null)
			{
				empty.X = (float)TmxHelper.GetAttributeAsInt(xElement, "x");
				empty.Y = (float)TmxHelper.GetAttributeAsInt(xElement, "y");
			}
			tileset.TileOffset = empty;
			List<TmxTile> list = new List<TmxTile>();
			TmxProperties tmxProperties = TmxProperties.FromXml(xml);
			string propertyValueAsString = tmxProperties.GetPropertyValueAsString("unity:namePrefix", "");
			string propertyValueAsString2 = tmxProperties.GetPropertyValueAsString("unity:namePostfix", "");
			if (xml.Element("image") != null)
			{
				TmxImage tmxImage = TmxImage.FromXml(xml.Element("image"), propertyValueAsString, propertyValueAsString2);
				for (int i = tileset.Margin + tileset.TileHeight; i <= tmxImage.Size.Height; i += tileset.Spacing + tileset.TileHeight)
				{
					for (int j = tileset.Margin + tileset.TileWidth; j <= tmxImage.Size.Width; j += tileset.Spacing + tileset.TileWidth)
					{
						uint num = (uint)list.Count();
						uint globalId = tileset.FirstGlobalId + num;
						TmxTile tmxTile = new TmxTile(tileset.TmxMap, globalId, num, tileset.Name, tmxImage);
						tmxTile.Offset = empty;
						tmxTile.SetTileSize(tileset.TileWidth, tileset.TileHeight);
						tmxTile.SetLocationOnSource(j - tileset.TileWidth, i - tileset.TileHeight);
						list.Add(tmxTile);
					}
				}
			}
			else
			{
				foreach (XElement item in xml.Elements("tile"))
				{
					TmxImage tmxImage2 = TmxImage.FromXml(item.Element("image"), propertyValueAsString, propertyValueAsString2);
					uint defaultValue = (uint)list.Count();
					defaultValue = TmxHelper.GetAttributeAsUInt(item, "id", defaultValue);
					uint globalId2 = tileset.FirstGlobalId + defaultValue;
					TmxTile tmxTile2 = new TmxTile(tileset.TmxMap, globalId2, defaultValue, tileset.Name, tmxImage2);
					tmxTile2.Offset = empty;
					tmxTile2.SetTileSize(tmxImage2.Size.Width, tmxImage2.Size.Height);
					tmxTile2.SetLocationOnSource(0, 0);
					list.Add(tmxTile2);
				}
			}
			tileset.Tiles.AddRange(list);
			Logger.WriteVerbose("Added {0} tiles", list.Count);
			foreach (XElement item2 in xml.Elements("tile"))
			{
				int localTileId = TmxHelper.GetAttributeAsInt(item2, "id");
				IEnumerable<TmxTile> source = from t in tileset.Tiles
				where t.GlobalId == localTileId + tileset.FirstGlobalId
				select t;
				if (source.Count() == 0)
				{
					Logger.WriteWarning("Tile '{0}' in tileset '{1}' does not exist but there is tile data for it.\n{2}", localTileId, tileset.Name, item2.ToString());
				}
				else
				{
					source.First().ParseTileXml(item2, tileset.TmxMap, tileset.FirstGlobalId);
				}
			}
		}

		private static void ParseTilesetSource(string tsxSource, TsxTileset tileset)
		{
			tileset.Source = Path.GetFullPath(tsxSource);
			if (File.Exists(tileset.Source))
			{
				using (new ChDir(tileset.Source))
				{
					ParseTilesetXml(TmxMap.LoadDocument(tileset.Source).Root, tileset);
				}
			}
			else
			{
				Logger.WriteError("Tileset file does not exist: {0}", tileset.Source);
			}
		}
	}
}
