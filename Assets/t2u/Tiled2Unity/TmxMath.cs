using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Tiled2Unity
{
	public class TmxMath
	{
		public static readonly uint FLIPPED_HORIZONTALLY_FLAG = 2147483648u;

		public static readonly uint FLIPPED_VERTICALLY_FLAG = 1073741824u;

		public static readonly uint FLIPPED_DIAGONALLY_FLAG = 536870912u;

		public static uint GetTileIdWithoutFlags(uint tileId)
		{
			return tileId & ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG);
		}

		public static bool IsTileFlippedDiagonally(uint tileId)
		{
			return (tileId & FLIPPED_DIAGONALLY_FLAG) != 0;
		}

		public static bool IsTileFlippedHorizontally(uint tileId)
		{
			return (tileId & FLIPPED_HORIZONTALLY_FLAG) != 0;
		}

		public static bool IsTileFlippedVertically(uint tileId)
		{
			return (tileId & FLIPPED_VERTICALLY_FLAG) != 0;
		}

		public static void RotatePoints(PointF[] points, TmxObject tmxObject)
		{
			TranslatePoints(points, 0f - tmxObject.Position.X, 0f - tmxObject.Position.Y);
			new TmxRotationMatrix(0f - tmxObject.Rotation).TransformPoints(points);
			TranslatePoints(points, tmxObject.Position.X, tmxObject.Position.Y);
		}

		public static void TransformPoints(PointF[] points, PointF origin, bool diagonal, bool horizontal, bool vertical)
		{
			TranslatePoints(points, 0f - origin.X, 0f - origin.Y);
			TmxRotationMatrix tmxRotationMatrix = new TmxRotationMatrix();
			if (horizontal)
			{
				tmxRotationMatrix = TmxRotationMatrix.Multiply(new TmxRotationMatrix(-1f, 0f, 0f, 1f), tmxRotationMatrix);
			}
			if (vertical)
			{
				tmxRotationMatrix = TmxRotationMatrix.Multiply(new TmxRotationMatrix(1f, 0f, 0f, -1f), tmxRotationMatrix);
			}
			if (diagonal)
			{
				tmxRotationMatrix = TmxRotationMatrix.Multiply(new TmxRotationMatrix(0f, 1f, 1f, 0f), tmxRotationMatrix);
			}
			tmxRotationMatrix.TransformPoints(points);
			TranslatePoints(points, origin.X, origin.Y);
		}

		public static void TransformPoints_DiagFirst(PointF[] points, PointF origin, bool diagonal, bool horizontal, bool vertical)
		{
			TranslatePoints(points, 0f - origin.X, 0f - origin.Y);
			TmxRotationMatrix tmxRotationMatrix = new TmxRotationMatrix();
			if (diagonal)
			{
				tmxRotationMatrix = TmxRotationMatrix.Multiply(new TmxRotationMatrix(0f, 1f, 1f, 0f), tmxRotationMatrix);
			}
			if (horizontal)
			{
				tmxRotationMatrix = TmxRotationMatrix.Multiply(new TmxRotationMatrix(-1f, 0f, 0f, 1f), tmxRotationMatrix);
			}
			if (vertical)
			{
				tmxRotationMatrix = TmxRotationMatrix.Multiply(new TmxRotationMatrix(1f, 0f, 0f, -1f), tmxRotationMatrix);
			}
			tmxRotationMatrix.TransformPoints(points);
			TranslatePoints(points, origin.X, origin.Y);
		}

		public static void TranslatePoints(PointF[] points, float tx, float ty)
		{
			TranslatePoints(points, new PointF(tx, ty));
		}

		public static void TranslatePoints(PointF[] points, PointF translate)
		{
			SizeF sz = new SizeF(translate.X, translate.Y);
			for (int i = 0; i < points.Length; i++)
			{
				points[i] = PointF.Add(points[i], sz);
			}
		}

		public static bool DoStaggerX(TmxMap tmxMap, int x)
		{
			int num = (tmxMap.StaggerAxis == TmxMap.MapStaggerAxis.X) ? 1 : 0;
			int num2 = (tmxMap.StaggerIndex == TmxMap.MapStaggerIndex.Even) ? 1 : 0;
			if (num != 0)
			{
				return ((x & 1) ^ num2) != 0;
			}
			return false;
		}

		public static bool DoStaggerY(TmxMap tmxMap, int y)
		{
			int num = (tmxMap.StaggerAxis == TmxMap.MapStaggerAxis.X) ? 1 : 0;
			int num2 = (tmxMap.StaggerIndex == TmxMap.MapStaggerIndex.Even) ? 1 : 0;
			if (num == 0)
			{
				return ((y & 1) ^ num2) != 0;
			}
			return false;
		}

		public static Point TileCornerFromGridCoordinates(TmxMap tmxMap, int x, int y)
		{
			if (tmxMap.Orientation == TmxMap.MapOrientation.Isometric)
			{
				Point empty = Point.Empty;
				int num = tmxMap.Height * tmxMap.TileWidth / 2;
				empty.X = (x - y) * tmxMap.TileWidth / 2 + num;
				empty.Y = (x + y) * tmxMap.TileHeight / 2;
				return empty;
			}
			if (tmxMap.Orientation == TmxMap.MapOrientation.Staggered || tmxMap.Orientation == TmxMap.MapOrientation.Hexagonal)
			{
				Point empty2 = Point.Empty;
				int num2 = tmxMap.TileWidth & -2;
				int num3 = tmxMap.TileHeight & -2;
				int num4 = (tmxMap.StaggerAxis == TmxMap.MapStaggerAxis.X) ? tmxMap.HexSideLength : 0;
				int num5 = (tmxMap.StaggerAxis == TmxMap.MapStaggerAxis.Y) ? tmxMap.HexSideLength : 0;
				int num6 = (num2 - num4) / 2;
				int num7 = (num3 - num5) / 2;
				int num8 = num6 + num4;
				int num9 = num7 + num5;
				if (tmxMap.StaggerAxis == TmxMap.MapStaggerAxis.X)
				{
					empty2.Y = y * (num3 + num5);
					if (DoStaggerX(tmxMap, x))
					{
						empty2.Y += num9;
					}
					empty2.X = x * num8;
				}
				else
				{
					empty2.X = x * (num2 + num4);
					if (DoStaggerY(tmxMap, y))
					{
						empty2.X += num8;
					}
					empty2.Y = y * num9;
				}
                empty2.X += num2/2;//+=(num2 / 2, 0);
                empty2.Y += 0;
                return empty2;
			}
			return new Point(x * tmxMap.TileWidth, y * tmxMap.TileHeight);
		}

		public static Point TileCornerInScreenCoordinates(TmxMap tmxMap, int x, int y)
		{
			Point result = TileCornerFromGridCoordinates(tmxMap, x, y);
			if (tmxMap.Orientation != 0)
			{
                result.X += -tmxMap.TileWidth;
                result.Y += 0;
            //	result.Offset(-tmxMap.TileWidth / 2, 0);
            }
			return result;
		}

		public static PointF ObjectPointFToMapSpace(TmxMap tmxMap, float x, float y)
		{
			return ObjectPointFToMapSpace(tmxMap, new PointF(x, y));
		}

		public static PointF ObjectPointFToMapSpace(TmxMap tmxMap, PointF pt)
		{
			if (tmxMap.Orientation == TmxMap.MapOrientation.Isometric)
			{
				PointF empty = PointF.Empty;
				float num = (float)(tmxMap.Height * tmxMap.TileWidth) * 0.5f;
				float num2 = pt.Y / (float)tmxMap.TileHeight;
				float num3 = pt.X / (float)tmxMap.TileHeight;
				empty.X = (num3 - num2) * (float)tmxMap.TileWidth * 0.5f + num;
				empty.Y = (num3 + num2) * (float)tmxMap.TileHeight * 0.5f;
				return empty;
			}
			return pt;
		}

		public static Point AddPoints(Point a, Point b)
		{
			return new Point(a.X + b.X, a.Y + b.Y);
		}

		public static PointF AddPoints(PointF a, PointF b)
		{
			return new PointF(a.X + b.X, a.Y + b.Y);
		}

		public static PointF MidPoint(PointF a, PointF b)
		{
			float x = (a.X + b.X) * 0.5f;
			float y = (a.Y + b.Y) * 0.5f;
			return new PointF(x, y);
		}

		public static PointF ScalePoint(PointF p, float s)
		{
			return new PointF(p.X * s, p.Y * s);
		}

		public static PointF ScalePoint(float x, float y, float s)
		{
			return new PointF(x * s, y * s);
		}

		public static PointF AddPointsScale(PointF a, PointF b, float scale)
		{
			return new PointF(a.X + b.X * scale, a.Y + b.Y * scale);
		}

		public static List<PointF> GetPointsInMapSpace(TmxMap tmxMap, TmxHasPoints objectWithPoints)
		{
			PointF local = ObjectPointFToMapSpace(tmxMap, 0f, 0f);
			local.X = 0f - local.X;
			local.Y = 0f - local.Y;
			return (from pt in (from pt in objectWithPoints.Points
			select ObjectPointFToMapSpace(tmxMap, pt)).ToList()
			select AddPoints(pt, local)).ToList();
		}

		public static float Sanitize(float v)
		{
			return (float)Math.Round((double)(v * 256f)) / 256f;
		}

		public static PointF Sanitize(PointF pt)
		{
			return new PointF(Sanitize(pt.X), Sanitize(pt.Y));
		}
	}
}
