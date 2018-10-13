using ClipperLib;
using LibTessDotNet;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Tiled2Unity.Geometry
{
	public class TriangulateClipperSolution
	{
		public List<PointF[]> Triangulate(PolyTree solution)
		{
			List<PointF[]> list = new List<PointF[]>();
			Tess tess = new Tess();
			tess.NoEmptyPolygons = true;
			Func<IntPoint, ContourVertex> selector = delegate(IntPoint p)
			{
				ContourVertex result = default(ContourVertex);
				result.Position = new Vec3
				{
					X = (float)p.X,
					Y = (float)p.Y,
					Z = 0f
				};
				return result;
			};
			for (PolyNode polyNode = solution.GetFirst(); polyNode != null; polyNode = polyNode.GetNext())
			{
				if (!polyNode.IsOpen)
				{
					ContourVertex[] vertices = polyNode.Contour.Select(selector).ToArray();
					tess.AddContour(vertices);
				}
			}
			tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);
			int elementCount = tess.ElementCount;
			for (int i = 0; i < elementCount; i++)
			{
				Vec3 position = tess.Vertices[tess.Elements[i * 3 + 0]].Position;
				Vec3 position2 = tess.Vertices[tess.Elements[i * 3 + 1]].Position;
				Vec3 position3 = tess.Vertices[tess.Elements[i * 3 + 2]].Position;
				List<PointF> list2 = new List<PointF>
				{
					new PointF(position.X, position.Y),
					new PointF(position2.X, position2.Y),
					new PointF(position3.X, position3.Y)
				};
				if (Math.Cross(list2[0], list2[1], list2[2]) > 0f)
				{
					list2.Reverse();
				}
				list.Add(list2.ToArray());
			}
			return list;
		}
	}
}
