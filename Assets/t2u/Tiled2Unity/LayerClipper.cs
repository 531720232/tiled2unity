using ClipperLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Tiled2Unity.Geometry;
using ClipperPolygon = System.Collections.Generic.List<ClipperLib.IntPoint>;
namespace Tiled2Unity
{
	public class LayerClipper
	{
		public delegate IntPoint TransformPointFunc(float x, float y);

		private static PolyFillType SubjectFillRule = PolyFillType.pftNonZero;

		private static PolyFillType ClipFillRule = PolyFillType.pftEvenOdd;

		public static PolyTree ExecuteClipper(TmxMap map, TmxChunk chunk, TransformPointFunc xfFunc)
		{

            ////for(int i=0;i<chunk.Height;i++)
            //// {
            ////     for(int j=0; j<chunk.Width;j++)
            ////     {
            ////         var raw = chunk.GetRawTileIdAt(j, i);
            ////      if(raw!=0)
            ////         {
            ////             var tid = TmxMath.GetTileIdWithoutFlags(raw);
            ////             var tile = map.Tiles[tid];
            ////             foreach(var p in tile.ObjectGroup.Objects)
            ////             {
            ////                 if(p is TmxHasPoints)
            ////                 {
            ////                     p.ToEnumerable().Where((x) =>
            ////                     {
            ////                         if (!usingUnityLayerOverride)
            ////                         {


            ////                             return string.Compare(tuple.Item1.Type, chunk.ParentData.ParentLayer.Name, true) == 0;
            ////                         }

            ////                         return true;
            ////                     });
            ////                 }

            ////             }
            ////         }
            ////     }
            //// }

            //     Clipper clipper = new Clipper(0);
            //     Tuple<TmxObject, TmxTile, uint> tuple = new Tuple<TmxObject, TmxTile, uint>(null, null, 0);
            //     bool usingUnityLayerOverride = !string.IsNullOrEmpty(chunk.ParentData.ParentLayer.UnityLayerOverrideName);
            //     foreach (var item2 in from h__TransparentIdentifier4 in (from y in Enumerable.Range(0, chunk.Height)
            //                                                              from x in Enumerable.Range(0, chunk.Width)
            //                                                              let rawTileId = chunk.GetRawTileIdAt(x, y)
            //                                                              where rawTileId != 0
            //                                                              let tileId = TmxMath.GetTileIdWithoutFlags(rawTileId)
            //                                                              let tile = map.Tiles[tileId]

            //                                                              from polygon in tile.ObjectGroup.Objects
            //                                                              where polygon is TmxHasPoints

            //                                                              select polygon.ToEnumerable().ToList().TrueForAll
            //                                                              (h__TransparentIdentifier4 =>

            //                    {
            //                        UnityEngine.Debug.Log("liudaodelh");
            //                        tuple = new Tuple<TmxObject, TmxTile, uint>(polygon, tile, rawTileId);
            //                        if (!usingUnityLayerOverride)
            //                        {


            //                            return string.Compare(tuple.Item1.Type, chunk.ParentData.ParentLayer.Name, true) == 0;
            //                        }

            //                        return true;
            //                    }))
            //                           select new
            //                           {

            //                               PositionOnMap = map.GetMapPositionAt((int)tuple.Item1.Position.X + chunk.X, (int)tuple.Item1.Position.Y + chunk.Y, tuple.Item2),
            //                               HasPointsInterface = (tuple.Item1 as TmxHasPoints),
            //                               TmxObjectInterface = tuple.Item1,
            //                               IsFlippedDiagnoally = TmxMath.IsTileFlippedDiagonally(tuple.Item3),
            //                               IsFlippedHorizontally = TmxMath.IsTileFlippedHorizontally(tuple.Item3),
            //                               IsFlippedVertically = TmxMath.IsTileFlippedVertically(tuple.Item3),
            //                               TileCenter = new PointF((float)tuple.Item2.TileSize.Width * 0.5f, (float)tuple.Item2.TileSize.Height * 0.5f)
            //                           })
            //     {
            //         List<IntPoint> list = new List<IntPoint>();
            //         SizeF offset = new SizeF(item2.TmxObjectInterface.Position);
            //         PointF[] array = item2.HasPointsInterface.Points.Select((PointF pt) => PointF.Add(pt, offset)).ToArray();
            //         TmxMath.TransformPoints(array, item2.TileCenter, item2.IsFlippedDiagnoally, item2.IsFlippedHorizontally, item2.IsFlippedVertically);
            //         PointF[] array2 = array;
            //         for (int i = 0; i < array2.Length; i++)
            //         {
            //             PointF pointF = array2[i];
            //             float x2 = (float)item2.PositionOnMap.X + pointF.X;
            //             float y2 = (float)item2.PositionOnMap.Y + pointF.Y;
            //             IntPoint item = xfFunc(x2, y2);
            //             list.Add(item);
            //         }
            //         list.Reverse();
            //         clipper.AddPath(list, PolyType.ptSubject, item2.HasPointsInterface.ArePointsClosed());
            //     }
            //     PolyTree polyTree = new PolyTree();
            //     clipper.Execute(ClipType.ctUnion, polyTree, SubjectFillRule, ClipFillRule);

            //     return polyTree;


            ClipperLib.Clipper clipper = new ClipperLib.Clipper();

            // Limit to polygon "type" that matches the collision layer name (unless we are overriding the whole layer to a specific Unity Layer Name)
            bool usingUnityLayerOverride = !String.IsNullOrEmpty(chunk.ParentData.ParentLayer.UnityLayerOverrideName);

            var polygons = from y in Enumerable.Range(0, chunk.Height)
                           from x in Enumerable.Range(0, chunk.Width)
                           let rawTileId = chunk.GetRawTileIdAt(x, y)
                           where rawTileId != 0
                           let tileId = TmxMath.GetTileIdWithoutFlags(rawTileId)
                           let tile = map.Tiles[tileId]
                           from polygon in tile.ObjectGroup.Objects
                           where (polygon as TmxHasPoints) != null
                           where usingUnityLayerOverride || String.Compare(polygon.Type, chunk.ParentData.ParentLayer.Name, true) == 0
                           select new
                           {
                               PositionOnMap = map.GetMapPositionAt(x + chunk.X, y + chunk.Y, tile),
                               HasPointsInterface = polygon as TmxHasPoints,
                               TmxObjectInterface = polygon,
                               IsFlippedDiagnoally = TmxMath.IsTileFlippedDiagonally(rawTileId),
                               IsFlippedHorizontally = TmxMath.IsTileFlippedHorizontally(rawTileId),
                               IsFlippedVertically = TmxMath.IsTileFlippedVertically(rawTileId),
                               TileCenter = new PointF(tile.TileSize.Width * 0.5f, tile.TileSize.Height * 0.5f),
                           };

            // Add all our polygons to the Clipper library so it can reduce all the polygons to a (hopefully small) number of paths
            foreach (var poly in polygons)
            {
                // Create a clipper library polygon out of each and add it to our collection
                ClipperPolygon clipperPolygon = new ClipperPolygon();

                // Our points may be transformed due to tile flipping/rotation
                // Before we transform them we put all the points into local space relative to the tile
                SizeF offset = new SizeF(poly.TmxObjectInterface.Position);
                PointF[] transformedPoints = poly.HasPointsInterface.Points.Select(pt => PointF.Add(pt, offset)).ToArray();

                // Now transform the points relative to the tile
                TmxMath.TransformPoints(transformedPoints, poly.TileCenter, poly.IsFlippedDiagnoally, poly.IsFlippedHorizontally, poly.IsFlippedVertically);

                foreach (var pt in transformedPoints)
                {
                    float x = poly.PositionOnMap.X + pt.X;
                    float y = poly.PositionOnMap.Y + pt.Y;

                    ClipperLib.IntPoint point = xfFunc(x, y);
                    clipperPolygon.Add(point);
                }

                // Because of Unity's cooridnate system, the winding order of the polygons must be reversed
                clipperPolygon.Reverse();

                // Add the "subject"
                clipper.AddPath(clipperPolygon, ClipperLib.PolyType.ptSubject, poly.HasPointsInterface.ArePointsClosed());
            }

            ClipperLib.PolyTree solution = new ClipperLib.PolyTree();
            clipper.Execute(ClipperLib.ClipType.ctUnion, solution, LayerClipper.SubjectFillRule, LayerClipper.ClipFillRule);
            return solution;
        }

		public static IEnumerable<PointF[]> SolutionPolygons_Complex(PolyTree solution)
            {
                List<List<IntPoint>>.Enumerator enumerator = Clipper.ClosedPathsFromPolyTree(solution).GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        IEnumerable<PointF> source = from pt in enumerator.Current
                                                     select new PointF((float)pt.X, (float)pt.Y);
                        yield return source.ToArray();
                    }
                }
                finally
                {
                    ((IDisposable)enumerator).Dispose();
                }
            
			enumerator = default(List<List<IntPoint>>.Enumerator);
		}

		public static IEnumerable<PointF[]> SolutionPolygons_Simple(PolyTree solution)
		{
			List<PointF[]> triangles = new TriangulateClipperSolution().Triangulate(solution);
			List<PointF[]> list = new ComposeConvexPolygons().Compose(triangles);
			List<PointF[]>.Enumerator enumerator = list.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					PointF[] current = enumerator.Current;
					yield return current;
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			enumerator = default(List<PointF[]>.Enumerator);
		}
	}
}
