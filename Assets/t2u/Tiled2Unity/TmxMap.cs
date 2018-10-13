using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Tiled2Unity
{
	public class TmxMap : TmxHasProperties, ITmxVisit
	{
		public enum MapOrientation
		{
			Orthogonal,
			Isometric,
			Staggered,
			Hexagonal
		}

		public enum MapStaggerAxis
		{
			X,
			Y
		}

		public enum MapStaggerIndex
		{
			Odd,
			Even
		}

		public Dictionary<uint, TmxTile> Tiles = new Dictionary<uint, TmxTile>();

		public Dictionary<uint, TgxTemplate> Templates = new Dictionary<uint, TgxTemplate>();

		public TmxObjectTypes ObjectTypes = new TmxObjectTypes();

		private uint nextUniqueId;

		public bool IsLoaded
		{
			get;
			private set;
		}

		public string Name
		{
			get;
			private set;
		}

		public MapOrientation Orientation
		{
			get;
			set;
		}

		public MapStaggerAxis StaggerAxis
		{
			get;
			private set;
		}

		public MapStaggerIndex StaggerIndex
		{
			get;
			private set;
		}

		public int HexSideLength
		{
			get;
			set;
		}

		public int DrawOrderHorizontal
		{
			get;
			private set;
		}

		public int DrawOrderVertical
		{
			get;
			private set;
		}

		public int Width
		{
			get;
			private set;
		}

		public int Height
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

		public Color BackgroundColor
		{
			get;
			private set;
		}

		public TmxProperties Properties
		{
			get;
			private set;
		}

		public Size MapSizeInPixels
		{
			get;
			private set;
		}

		public bool IsResource
		{
			get;
			private set;
		}

		public string ExplicitSortingLayerName
		{
			get;
			private set;
		}

		public List<TsxTileset> Tilesets
		{
			get;
			private set;
		}

		public List<TgxTemplateGroup> TemplateGroups
		{
			get;
			private set;
		}

		public List<TmxLayerNode> LayerNodes
		{
			get;
			private set;
		}

		public TmxMap()
		{
			IsLoaded = false;
			Tilesets = new List<TsxTileset>();
			TemplateGroups = new List<TgxTemplateGroup>();
			Properties = new TmxProperties();
			ExplicitSortingLayerName = "";
			LayerNodes = new List<TmxLayerNode>();
		}

		public TmxMap MakeTemplate(string name)
		{
			return new TmxMap
			{
				Name = Name + "-" + name
			};
		}

		public string GetExportedFilename()
		{
			return $"{Name}.tiled2unity.xml";
		}

		public override string ToString()
		{
			return string.Format("{{ \"{5}\" size = {0}x{1}, tile size = {2}x{3}, # tiles = {4} }}", Width, Height, TileWidth, TileHeight, Tiles.Count(), Name);
		}

		public TmxTile GetTileFromTileId(uint tileId)
		{
			if (tileId == 0)
			{
				return null;
			}
			tileId = TmxMath.GetTileIdWithoutFlags(tileId);
			return Tiles[tileId];
		}

		public Point GetMapPositionAt(int x, int y)
		{
			return TmxMath.TileCornerInScreenCoordinates(this, x, y);
		}

		public Point GetMapPositionAt(int x, int y, TmxTile tile)
		{
			Point mapPositionAt = GetMapPositionAt(x, y);
			mapPositionAt.X += (int)tile.Offset.X;
			mapPositionAt.Y += (int)tile.Offset.Y;
			mapPositionAt.Y = mapPositionAt.Y + TileHeight - tile.TileSize.Height;
			return mapPositionAt;
		}

		public uint GetUniqueId()
		{
			return ++nextUniqueId;
		}

		private Size CalculateMapSizeInPixels()
		{
			if (Orientation == MapOrientation.Isometric)
			{
				Size empty = Size.Empty;
				empty.Width = (Width + Height) * TileWidth / 2;
				empty.Height = (Width + Height) * TileHeight / 2;
				return empty;
			}
			if (Orientation == MapOrientation.Staggered || Orientation == MapOrientation.Hexagonal)
			{
				int num = TileHeight & -2;
				int num2 = TileWidth & -2;
				if (StaggerAxis == MapStaggerAxis.Y)
				{
					int num3 = (num - HexSideLength) / 2;
					Size empty2 = Size.Empty;
					empty2.Width = num2 * Width + num2 / 2;
					empty2.Height = (num3 + HexSideLength) * Height + num3;
					return empty2;
				}
				int num4 = (num2 - HexSideLength) / 2;
				Size empty3 = Size.Empty;
				empty3.Width = (num4 + HexSideLength) * Width + num4;
				empty3.Height = num * Height + num / 2;
				return empty3;
			}
			return new Size(Width * TileWidth, Height * TileHeight);
		}

		public List<TmxMesh> GetUniqueListOfVisibleObjectTileMeshes()
		{
			return (from objectGroup in EnumerateObjectLayers()
			where objectGroup.Visible
			from tmxObject in objectGroup.Objects
			where tmxObject.Visible
			let tmxObjectTile = tmxObject as TmxObjectTile
			where tmxObjectTile != null
			from tmxMesh in tmxObjectTile.Tile.Meshes
			select tmxMesh into m
			group m by m.UniqueMeshName into g
			select g.First()).ToList();
		}

		public void LoadObjectTypeXml(string xmlPath)
		{
			if (string.IsNullOrEmpty(xmlPath))
			{
				Logger.WriteInfo("Object Type XML file is not being used.");
			}
			else
			{
				Logger.WriteInfo("Loading Object Type Xml file: '{0}'", xmlPath);
				try
				{
					ObjectTypes = TmxObjectTypes.FromXmlFile(xmlPath);
				}
				catch (FileNotFoundException)
				{
					Logger.WriteError("Object Type Xml file was not found: {0}", xmlPath);
					ObjectTypes = new TmxObjectTypes();
				}
				catch (Exception ex2)
				{
					Logger.WriteError("Error parsing Object Type Xml file: {0}\n{1}", xmlPath, ex2.Message);
					Logger.WriteError("Stack:\n{0}", ex2.StackTrace);
					ObjectTypes = new TmxObjectTypes();
				}
				Logger.WriteInfo("Tiled Object Type count = {0}", ObjectTypes.TmxObjectTypeMapping.Count());
			}
		}

		public void ClearObjectTypeXml()
		{
			Logger.WriteInfo("Removing Object Types from map.");
			ObjectTypes = new TmxObjectTypes();
		}

		public IEnumerable<TmxLayer> EnumerateTileLayers()
		{
			foreach (TmxLayer item in EnumerateLayersByType<TmxLayer>())
			{
				yield return item;
			}
		}

		public IEnumerable<TmxObjectGroup> EnumerateObjectLayers()
		{
			foreach (TmxObjectGroup item in EnumerateLayersByType<TmxObjectGroup>())
			{
				yield return item;
			}
		}

		private IEnumerable<T> EnumerateLayersByType<T>() where T : TmxLayerNode
		{
			List<TmxLayerNode>.Enumerator enumerator = LayerNodes.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TmxLayerNode current = enumerator.Current;
					foreach (T item in RecursiveEnumerate<T>(current))
					{
						yield return item;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			enumerator = default(List<TmxLayerNode>.Enumerator);
		}

		private IEnumerable<T> RecursiveEnumerate<T>(TmxLayerNode layerNode) where T : TmxLayerNode
		{
			if (layerNode.GetType() == typeof(T))
			{
				yield return (T)layerNode;
			}
			List<TmxLayerNode>.Enumerator enumerator = layerNode.LayerNodes.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TmxLayerNode current = enumerator.Current;
					foreach (T item in RecursiveEnumerate<T>(current))
					{
						yield return item;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			enumerator = default(List<TmxLayerNode>.Enumerator);
		}

		public void Visit(ITmxVisitor visitor)
		{
			visitor.VisitMap(this);
			foreach (TmxLayerNode layerNode in LayerNodes)
			{
				layerNode.Visit(visitor);
			}
		}

		public static TmxMap LoadFromFile(string tmxPath)
		{
			string fullPath = Path.GetFullPath(tmxPath);
			using (new ChDir(fullPath))
			{
				TmxMap tmxMap = new TmxMap();
				XDocument doc = LoadDocument(fullPath);
				tmxMap.Name = Path.GetFileNameWithoutExtension(fullPath);
				tmxMap.ParseMapXml(doc);
				Logger.WriteInfo("Map details: {0}", tmxMap.ToString());
				Logger.WriteSuccess("Parsed: {0} ", fullPath);
				tmxMap.IsLoaded = true;
				return tmxMap;
			}
		}

		public static XDocument LoadDocument(string xmlPath)
		{
			XDocument xDocument = null;
			Logger.WriteInfo("Opening {0} ...", xmlPath);
			try
			{
				return XDocument.Load(xmlPath, LoadOptions.SetLineInfo);
			}
			catch (FileNotFoundException ex)
			{
				throw new TmxException($"File not found: {ex.FileName}", ex);
			}
			catch (XmlException ex2)
			{
				throw new TmxException($"Xml error in {xmlPath}\n  {ex2.Message}", ex2);
			}
		}

		private void ParseMapXml(XDocument doc)
		{
			Logger.WriteVerbose("Parsing map root ...");
			XElement xElement = doc.Element("map");
			try
			{
				Orientation = TmxHelper.GetAttributeAsEnum<MapOrientation>(xElement, "orientation");
				StaggerAxis = TmxHelper.GetAttributeAsEnum(xElement, "staggeraxis", MapStaggerAxis.Y);
				StaggerIndex = TmxHelper.GetAttributeAsEnum(xElement, "staggerindex", MapStaggerIndex.Odd);
				HexSideLength = TmxHelper.GetAttributeAsInt(xElement, "hexsidelength", 0);
				DrawOrderHorizontal = (TmxHelper.GetAttributeAsString(xElement, "renderorder", "right-down").Contains("right") ? 1 : (-1));
				DrawOrderVertical = (TmxHelper.GetAttributeAsString(xElement, "renderorder", "right-down").Contains("down") ? 1 : (-1));
				Width = TmxHelper.GetAttributeAsInt(xElement, "width");
				Height = TmxHelper.GetAttributeAsInt(xElement, "height");
				TileWidth = TmxHelper.GetAttributeAsInt(xElement, "tilewidth");
				TileHeight = TmxHelper.GetAttributeAsInt(xElement, "tileheight");
				BackgroundColor = TmxHelper.GetAttributeAsColor(xElement, "backgroundcolor", new Color32(128, 128, 128,255));
			}
			catch (Exception inner)
			{
				TmxException.FromAttributeException(inner, xElement);
			}
			Properties = TmxProperties.FromXml(xElement);
			IsResource = Properties.GetPropertyValueAsBoolean("unity:resource", false);
			IsResource = (IsResource || !string.IsNullOrEmpty(Properties.GetPropertyValueAsString("unity:resourcePath", null)));
			ExplicitSortingLayerName = Properties.GetPropertyValueAsString("unity:sortingLayerName", "");
			ParseAllTilesets(doc);
			ParseAllTemplates(doc);
			LayerNodes = TmxLayerNode.ListFromXml(xElement, null, this);
			MapSizeInPixels = CalculateMapSizeInPixels();
			TmxDisplayOrderVisitor visitor = new TmxDisplayOrderVisitor();
			Visit(visitor);
		}

		private void ParseAllTilesets(XDocument doc)
		{
			Logger.WriteVerbose("Parsing tileset elements ...");
			foreach (XElement item in from item in doc.Descendants("tileset")
			select (item))
			{
				TsxTileset tsxTileset = TsxTileset.FromXml(item, this);
				if (tsxTileset != null)
				{
					Tilesets.Add(tsxTileset);
				}
			}
			foreach (XElement item2 in from item in doc.Descendants("imagelayer")
			select (item))
			{
				TsxTileset tsxTileset2 = TsxTileset.FromImageLayerXml(item2, this);
				if (tsxTileset2 != null)
				{
					Tilesets.Add(tsxTileset2);
				}
			}
		}

		private void ParseAllTemplates(XDocument doc)
		{
			Logger.WriteVerbose("Parsing template group elements ...");
			foreach (XElement item in from item in doc.Descendants("templategroup")
			select (item))
			{
				TgxTemplateGroup tgxTemplateGroup = TgxTemplateGroup.FromXml(item, this);
				if (tgxTemplateGroup != null)
				{
					TemplateGroups.Add(tgxTemplateGroup);
				}
			}
		}
	}
}
