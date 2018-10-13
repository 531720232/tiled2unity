using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;

namespace Tiled2Unity
{
	public class TmxLayer : TmxLayerNode
	{
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

		public TmxData Data
		{
			get;
			private set;
		}

		public List<TmxMesh> Meshes
		{
			get;
			private set;
		}

		public List<TmxLayer> CollisionLayers
		{
			get;
			private set;
		}

		public TmxLayer(TmxLayerNode parent, TmxMap map)
			: base(parent, map)
		{
			base.Visible = true;
			base.Opacity = 1f;
			Data = new TmxData(this);
			CollisionLayers = new List<TmxLayer>();
		}

		public bool IsExportingConvexPolygons()
		{
			if (base.Properties.PropertyMap.ContainsKey("unity:convex"))
			{
				return base.Properties.GetPropertyValueAsBoolean("unity:convex", true);
			}
			if (base.ParentMap.Properties.PropertyMap.ContainsKey("unity:convex"))
			{
				return base.ParentMap.Properties.GetPropertyValueAsBoolean("unity:convex", true);
			}
			return Settings.PreferConvexPolygons;
		}

		public override void Visit(ITmxVisitor visitor)
		{
			visitor.VisitTileLayer(this);
		}

		public static TmxLayer FromXml(XElement elem, TmxLayerNode parent, TmxMap tmxMap)
		{
			TmxLayer tmxLayer = new TmxLayer(parent, tmxMap);
			tmxLayer.FromXmlInternal(elem);
			if (elem.Name == (XName)"layer")
			{
				tmxLayer.ParseLayerXml(elem);
			}
			else if (elem.Name == (XName)"imagelayer")
			{
				tmxLayer.ParseImageLayerXml(elem);
			}
			tmxLayer.Meshes = TmxMesh.ListFromTmxLayer(tmxLayer);
			tmxLayer.BuildCollisionLayers();
			return tmxLayer;
		}

		private void ParseLayerXml(XElement xml)
		{
			Width = TmxHelper.GetAttributeAsInt(xml, "width");
			Height = TmxHelper.GetAttributeAsInt(xml, "height");
			ParseData(xml.Element("data"));
		}

		private void ParseImageLayerXml(XElement xml)
		{
			if (xml.Element("image") == null)
			{
				Logger.WriteWarning("Image Layer '{0}' is being ignored since it has no image.", base.Name);
				base.Ignore = IgnoreSettings.True;
			}
			else
			{
				Width = 1;
				Height = 1;
				string imagePath = TmxHelper.GetAttributeAsFullPath(xml.Element("image"), "source");
				TmxTile value = base.ParentMap.Tiles.First((KeyValuePair<uint, TmxTile> t) => t.Value.TmxImage.AbsolutePath.ToLower() == imagePath.ToLower()).Value;
				Data = new TmxData(this);
				TmxChunk tmxChunk = new TmxChunk(Data);
				tmxChunk.X = 0;
				tmxChunk.Y = 0;
				tmxChunk.Width = 1;
				tmxChunk.Height = 1;
				tmxChunk.TileIds.Add(value.GlobalId);
				Data.Chunks.Add(tmxChunk);
				PointF offset = base.Offset;
				offset.Y -= (float)base.ParentMap.TileHeight;
				offset.Y += (float)value.TmxImage.Size.Height;
				PointF pointF = TmxMath.TileCornerInScreenCoordinates(base.ParentMap, 0, 0);
				offset.X -= pointF.X;
				offset.Y -= pointF.Y;
				offset.X += TmxHelper.GetAttributeAsFloat(xml, "x", 0f);
				offset.Y += TmxHelper.GetAttributeAsFloat(xml, "y", 0f);
				base.Offset = offset;
			}
		}

		private void ParseData(XElement elem)
		{
			Logger.WriteVerbose("Parse {0} layer data ...", base.Name);
			Data = TmxData.FromDataXml(elem, this);
		}

		private void BuildCollisionLayers()
		{
			CollisionLayers.Clear();
			if (base.Visible && base.Ignore != IgnoreSettings.True && base.Ignore != IgnoreSettings.Collision)
			{
				if (string.IsNullOrEmpty(base.UnityLayerOverrideName))
				{
					BuildCollisionLayers_ByObjectType();
				}
				else
				{
					BuildCollisionLayers_Override();
				}
			}
		}

		private void BuildCollisionLayers_Override()
		{
			CollisionLayers.Clear();
			CollisionLayers.Add(this);
		}

		private void BuildCollisionLayers_ByObjectType()
		{
			for (int i = 0; i < Data.Chunks.Count; i++)
			{
				TmxChunk tmxChunk = Data.Chunks[i];
				for (int j = 0; j < tmxChunk.TileIds.Count; j++)
				{
					uint num = tmxChunk.TileIds[j];
					if (num != 0)
					{
						uint tileIdWithoutFlags = TmxMath.GetTileIdWithoutFlags(num);
						foreach (TmxObject @object in base.ParentMap.Tiles[tileIdWithoutFlags].ObjectGroup.Objects)
						{
							if (@object is TmxHasPoints)
							{
								TmxLayer tmxLayer = CollisionLayers.Find((TmxLayer l) => string.Compare(l.Name, @object.Type, true) == 0);
								if (tmxLayer == null)
								{
									tmxLayer = new TmxLayer(null, base.ParentMap);
									CollisionLayers.Add(tmxLayer);
									tmxLayer.Name = @object.Type;
									tmxLayer.Offset = base.Offset;
									tmxLayer.Width = Width;
									tmxLayer.Height = Height;
									tmxLayer.Ignore = base.Ignore;
									tmxLayer.Properties = base.Properties;
									tmxLayer.Data = Data.MakeEmptyCopy(tmxLayer);
								}
								tmxLayer.Data.Chunks[i].TileIds[j] = num;
							}
						}
					}
				}
			}
		}
	}
}
