using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace Tiled2Unity
{
	public class TmxObjectTile : TmxObject
	{
		public TmxTile Tile
		{
			get;
			private set;
		}

		public bool FlippedHorizontal
		{
			get;
			private set;
		}

		public bool FlippedVertical
		{
			get;
			private set;
		}

		public int DrawOrderIndex
		{
			get;
			set;
		}

		public int DepthIndex
		{
			get;
			set;
		}

		private string ExplicitSortingLayerName
		{
			get;
			set;
		}

		private int? ExplicitSortingOrder
		{
			get;
			set;
		}

		public TmxObjectTile()
		{
			DrawOrderIndex = -1;
			DepthIndex = -1;
			ExplicitSortingLayerName = "";
		}

		public override Rect GetWorldBounds()
		{
            Rect rectangleF = new Rect(base.Position.X, base.Position.Y - base.Size.Height, base.Size.Width, base.Size.Height);
            Rect worldBounds = Tile.ObjectGroup.GetWorldBounds(base.Position);
			if (worldBounds == Rect.zero)
			{
				return rectangleF;
			}
          
			return RectEx.Union(rectangleF,worldBounds);
		}

		public override string ToString()
		{
			return $"{{ TmxObjectTile: name={GetNonEmptyName()}, pos={base.Position}, tile={Tile} }}";
		}

		public SizeF GetTileObjectScale()
		{
			float width = base.Size.Width / (float)Tile.TileSize.Width;
			float height = base.Size.Height / (float)Tile.TileSize.Height;
			return new SizeF(width, height);
		}

		public string GetSortingLayerName()
		{
			if (!string.IsNullOrEmpty(ExplicitSortingLayerName))
			{
				return ExplicitSortingLayerName;
			}
			return base.ParentObjectGroup.GetSortingLayerName();
		}

		public int GetSortingOrder()
		{
			if (ExplicitSortingOrder.HasValue)
			{
				return ExplicitSortingOrder.Value;
			}
			return DrawOrderIndex;
		}

		protected override void InternalFromXml(XElement xml, TmxMap tmxMap)
		{
			uint attributeAsUInt = TmxHelper.GetAttributeAsUInt(xml, "gid");
			FlippedHorizontal = TmxMath.IsTileFlippedHorizontally(attributeAsUInt);
			FlippedVertical = TmxMath.IsTileFlippedVertically(attributeAsUInt);
			uint tileIdWithoutFlags = TmxMath.GetTileIdWithoutFlags(attributeAsUInt);
			Tile = tmxMap.Tiles[tileIdWithoutFlags];
			if (Tile.Meshes.Count() == 0)
			{
				Tile.Meshes = TmxMesh.FromTmxTile(Tile, tmxMap);
			}
			ExplicitSortingLayerName = base.Properties.GetPropertyValueAsString("unity:sortingLayerName", "");
			if (base.Properties.PropertyMap.ContainsKey("unity:sortingOrder"))
			{
				ExplicitSortingOrder = base.Properties.GetPropertyValueAsInt("unity:sortingOrder");
			}
		}

		protected override string InternalGetDefaultName()
		{
			return "TileObject";
		}
	}
}
