using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;

namespace Tiled2Unity
{
	public class TmxTile : TmxHasProperties
	{
		public TmxMap TmxMap
		{
			get;
			private set;
		}

		public uint GlobalId
		{
			get;
			private set;
		}

		public uint LocalId
		{
			get;
			private set;
		}

		public Size TileSize
		{
			get;
			private set;
		}

		public PointF Offset
		{
			get;
			set;
		}

		public TmxImage TmxImage
		{
			get;
			private set;
		}

		public Point LocationOnSource
		{
			get;
			private set;
		}

		public TmxProperties Properties
		{
			get;
			private set;
		}

		public TmxObjectGroup ObjectGroup
		{
			get;
			private set;
		}

		public TmxAnimation Animation
		{
			get;
			private set;
		}

		public List<TmxMesh> Meshes
		{
			get;
			set;
		}

		public bool IsEmpty
		{
			get
			{
				if (GlobalId == 0)
				{
					return LocalId == 0;
				}
				return false;
			}
		}

		public TmxTile(TmxMap tmxMap, uint globalId, uint localId, string tilesetName, TmxImage tmxImage)
		{
			TmxMap = TmxMap;
			GlobalId = globalId;
			LocalId = localId;
			TmxImage = tmxImage;
			Properties = new TmxProperties();
			ObjectGroup = new TmxObjectGroup(null, TmxMap);
			Animation = TmxAnimation.FromTileId(globalId);
			Meshes = new List<TmxMesh>();
		}

		public void SetTileSize(int width, int height)
		{
			TileSize = new Size(width, height);
		}

		public void SetLocationOnSource(int x, int y)
		{
			LocationOnSource = new Point(x, y);
		}

		public override string ToString()
		{
			return $"{{id = {GlobalId}, source({LocationOnSource})}}";
		}

		public void ParseTileXml(XElement elem, TmxMap tmxMap, uint firstId)
		{
			Logger.WriteVerbose("Parse tile data (gid = {0}, id {1}) ...", GlobalId, LocalId);
			Properties = TmxProperties.FromXml(elem);
			XElement xElement = elem.Element("objectgroup");
			if (xElement != null)
			{
				ObjectGroup = TmxObjectGroup.FromXml(xElement, null, tmxMap);
				FixTileColliderObjects(tmxMap);
			}
			XElement xElement2 = elem.Element("animation");
			if (xElement2 != null)
			{
				Animation = TmxAnimation.FromXml(xElement2, firstId);
			}
		}

		private void FixTileColliderObjects(TmxMap tmxMap)
		{
			for (int i = 0; i < ObjectGroup.Objects.Count; i++)
			{
				TmxObject tmxObject = ObjectGroup.Objects[i];
				if (tmxObject is TmxObjectRectangle)
				{
					TmxObjectPolygon value = TmxObjectPolygon.FromRectangle(tmxMap, tmxObject as TmxObjectRectangle);
					ObjectGroup.Objects[i] = value;
				}
			}
			foreach (TmxObject @object in ObjectGroup.Objects)
			{
				TmxHasPoints tmxHasPoints = @object as TmxHasPoints;
				if (tmxHasPoints != null)
				{
					PointF[] array = tmxHasPoints.Points.ToArray();
					TmxMath.RotatePoints(array, @object);
					array = array.Select(TmxMath.Sanitize).ToArray();
					tmxHasPoints.Points = array.ToList();
					@object.BakeRotation();
				}
			}
		}
	}
}
