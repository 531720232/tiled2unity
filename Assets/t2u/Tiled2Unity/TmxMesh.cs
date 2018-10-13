using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tiled2Unity
{
	public class TmxMesh
	{
		private static readonly int MaxNumberOfTiles = 16250;

		private string uniqueMeshName;

		public string UniqueMeshName
		{
			get
			{
				return uniqueMeshName;
			}
			private set
			{
				uniqueMeshName = value.Replace(" ", "-");
			}
		}

		public string ObjectName
		{
			get;
			private set;
		}

		public TmxImage TmxImage
		{
			get;
			private set;
		}

		public int X
		{
			get;
			private set;
		}

		public int Y
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

		public uint[] TileIds
		{
			get;
			private set;
		}

		public int StartingTileIndex
		{
			get;
			private set;
		}

		public int NumberOfTiles
		{
			get;
			private set;
		}

		public int StartTimeMs
		{
			get;
			private set;
		}

		public int DurationMs
		{
			get;
			private set;
		}

		public int FullAnimationDurationMs
		{
			get;
			private set;
		}

		private TmxMesh()
		{
		}

		public bool IsMeshFull()
		{
			return NumberOfTiles >= MaxNumberOfTiles;
		}

		public int GetTileIndex(int x, int y)
		{
			return y * Width + x;
		}

		public uint GetTileIdAt(int tileIndex)
		{
			int num = tileIndex - StartingTileIndex;
			if (num < 0 || num >= TileIds.Length)
			{
				return 0u;
			}
			return TileIds[num];
		}

		private void AddTile(int index, uint tileId)
		{
			TileIds[index] = tileId;
			NumberOfTiles++;
			if (IsMeshFull())
			{
				List<uint> list = TileIds.ToList();
				int num = list.FindIndex((uint t) => t != 0);
				if (num > 0)
				{
					StartingTileIndex = num;
					list.RemoveRange(0, num);
				}
				list.Reverse();
				num = list.FindIndex((uint t) => t != 0);
				if (num > 0)
				{
					list.RemoveRange(0, num);
				}
				list.Reverse();
				TileIds = list.ToArray();
			}
		}

		public static List<TmxMesh> ListFromTmxLayer(TmxLayer layer)
		{
			List<TmxMesh> list = new List<TmxMesh>();
			foreach (TmxChunk chunk in layer.Data.Chunks)
			{
				list.AddRange(ListFromTmxChunk(chunk));
			}
			return list;
		}

		public static List<TmxMesh> ListFromTmxChunk(TmxChunk chunk)
		{
			List<TmxMesh> list = new List<TmxMesh>();
			for (int i = 0; i < chunk.TileIds.Count; i++)
			{
				uint num = chunk.TileIds[i];
				TmxTile tile = chunk.ParentData.ParentLayer.ParentMap.GetTileFromTileId(num);
				if (tile != null)
				{
					int timeMs = 0;
					foreach (TmxFrame frame in tile.Animation.Frames)
					{
						uint globalTileId = frame.GlobalTileId;
						globalTileId |= (num & TmxMath.FLIPPED_HORIZONTALLY_FLAG);
						globalTileId |= (num & TmxMath.FLIPPED_VERTICALLY_FLAG);
						globalTileId |= (num & TmxMath.FLIPPED_DIAGONALLY_FLAG);
						TmxMesh tmxMesh = list.Find((TmxMesh m) => m.CanAddFrame(tile, timeMs, frame.DurationMs, tile.Animation.TotalTimeMs));
						if (tmxMesh == null)
						{
							TmxTile tileFromTileId = chunk.ParentData.ParentLayer.ParentMap.GetTileFromTileId(globalTileId);
							tmxMesh = new TmxMesh();
							tmxMesh.X = chunk.X;
							tmxMesh.Y = chunk.Y;
							tmxMesh.Width = chunk.Width;
							tmxMesh.Height = chunk.Height;
							tmxMesh.TileIds = new uint[chunk.TileIds.Count];
							tmxMesh.UniqueMeshName = string.Format("{0}_mesh_{1}", chunk.ParentData.ParentLayer.ParentMap.Name, chunk.ParentData.ParentLayer.ParentMap.GetUniqueId().ToString("D4"));
							tmxMesh.TmxImage = tileFromTileId.TmxImage;
							tmxMesh.StartTimeMs = timeMs;
							tmxMesh.DurationMs = frame.DurationMs;
							tmxMesh.FullAnimationDurationMs = tile.Animation.TotalTimeMs;
							tmxMesh.ObjectName = Path.GetFileNameWithoutExtension(tileFromTileId.TmxImage.AbsolutePath);
							if (tmxMesh.DurationMs != 0)
							{
								TmxMesh tmxMesh2 = tmxMesh;
								tmxMesh2.ObjectName += $"[{timeMs}-{timeMs + tmxMesh.DurationMs}][{tmxMesh.FullAnimationDurationMs}]";
							}
							list.Add(tmxMesh);
						}
						tmxMesh.AddTile(i, globalTileId);
						timeMs += frame.DurationMs;
					}
				}
			}
			return list;
		}

		public static List<TmxMesh> FromTmxTile(TmxTile tmxTile, TmxMap tmxMap)
		{
			List<TmxMesh> list = new List<TmxMesh>();
			int num = 0;
			foreach (TmxFrame frame in tmxTile.Animation.Frames)
			{
				uint globalTileId = frame.GlobalTileId;
				TmxTile tmxTile2 = tmxMap.Tiles[globalTileId];
				TmxMesh tmxMesh = new TmxMesh();
				tmxMesh.TileIds = new uint[1];
				tmxMesh.TileIds[0] = globalTileId;
				tmxMesh.UniqueMeshName = string.Format("{0}_mesh_tile_{1}", tmxMap.Name, TmxMath.GetTileIdWithoutFlags(globalTileId).ToString("D4"));
				tmxMesh.TmxImage = tmxTile2.TmxImage;
				tmxMesh.ObjectName = "tile_obj";
				tmxMesh.StartTimeMs = num;
				tmxMesh.DurationMs = frame.DurationMs;
				tmxMesh.FullAnimationDurationMs = tmxTile.Animation.TotalTimeMs;
				if (tmxMesh.DurationMs != 0)
				{
					TmxMesh tmxMesh2 = tmxMesh;
					tmxMesh2.ObjectName += $"[{num}-{num + tmxMesh.DurationMs}][{tmxMesh.FullAnimationDurationMs}]";
				}
				num += frame.DurationMs;
				list.Add(tmxMesh);
			}
			return list;
		}

		private bool CanAddFrame(TmxTile tile, int startMs, int durationMs, int totalTimeMs)
		{
			if (IsMeshFull())
			{
				return false;
			}
			if (TmxImage != tile.TmxImage)
			{
				return false;
			}
			if (StartTimeMs != startMs)
			{
				return false;
			}
			if (DurationMs != durationMs)
			{
				return false;
			}
			if (FullAnimationDurationMs != totalTimeMs)
			{
				return false;
			}
			return true;
		}
	}
}
