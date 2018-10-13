using ClipperLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace Tiled2Unity
{
    public class TiledMapExporter
    {
        private class TmxImageComparer : IEqualityComparer<TmxImage>
        {
            public bool Equals(TmxImage lhs, TmxImage rhs)
            {
                return lhs.AbsolutePath.ToLower() == rhs.AbsolutePath.ToLower();
            }

            public int GetHashCode(TmxImage tmxImage)
            {
                return tmxImage.AbsolutePath.ToLower().GetHashCode();
            }
        }

        public struct Vertex3
        {
            public float X
            {
                get;
                set;
            }

            public float Y
            {
                get;
                set;
            }

            public float Z
            {
                get;
                set;
            }

            public static Vertex3 FromPointF(PointF point, float depth)
            {
                Vertex3 result = default(Vertex3);
                result.X = point.X;
                result.Y = point.Y;
                result.Z = depth;
                return result;
            }
        }

        public struct FaceVertices
        {
            public PointF[] Vertices
            {
                get;
                set;
            }

            public float Depth_z
            {
                get;
                set;
            }

            public Vertex3 V0 => Vertex3.FromPointF(Vertices[0], Depth_z);

            public Vertex3 V1 => Vertex3.FromPointF(Vertices[1], Depth_z);

            public Vertex3 V2 => Vertex3.FromPointF(Vertices[2], Depth_z);

            public Vertex3 V3 => Vertex3.FromPointF(Vertices[3], Depth_z);
        }

        public enum PrefabContext
        {
            Root,
            TiledLayer,
            ObjectLayer,
            Object
        }

        private delegate void TransformVerticesFunc(PointF[] verts);

        private static readonly int MaxNumberOfSafePaths = 256;

        private TmxMap tmxMap;

        private Dictionary<string, string> mtm = new Dictionary<string, string>();
        private List<XElement> CreateAssignMaterialsElements()
        {
            List<XElement> list = new List<XElement>();
            List<TmxMesh>.Enumerator enumerator2;
            foreach (TmxLayer item3 in tmxMap.EnumerateTileLayers())
            {
                if (item3.Visible && item3.Ignore != TmxLayerNode.IgnoreSettings.Visual)
                {
                    enumerator2 = item3.Meshes.GetEnumerator();
                    try
                    {
                        while (enumerator2.MoveNext())
                        {
                            TmxMesh current2 = enumerator2.Current;
                            XElement item = new XElement("AssignMaterial", new XAttribute("mesh", current2.UniqueMeshName), new XAttribute("material", Path.GetFileNameWithoutExtension(current2.TmxImage.AbsolutePath)));
                            list.Add(item);
                            mtm[current2.UniqueMeshName] = Path.GetFileNameWithoutExtension(current2.TmxImage.AbsolutePath);
                        }
                    }
                    finally
                    {
                        ((IDisposable)enumerator2).Dispose();
                    }
                }
            }
            enumerator2 = tmxMap.GetUniqueListOfVisibleObjectTileMeshes().GetEnumerator();
            try
            {
                while (enumerator2.MoveNext())
                {
                    TmxMesh current3 = enumerator2.Current;
                    XElement item2 = new XElement("AssignMaterial", new XAttribute("mesh", current3.UniqueMeshName), new XAttribute("material", Path.GetFileNameWithoutExtension(current3.TmxImage.AbsolutePath)));
                    list.Add(item2);
                }
                return list;
            }
            finally
            {
                ((IDisposable)enumerator2).Dispose();
            }
        }

        private UnityEngine.GameObject CreateCollisionElementForLayer(TmxLayer layer)
        {
            List<XElement> list = new List<XElement>();
            var obj = new UnityEngine.GameObject("Collision");
            LayerClipper.TransformPointFunc xfFunc = delegate (float x, float y)
            {
                PointF pointF = PointFToUnityVector_NoScale(new PointF(x, y));
                return new IntPoint((double)pointF.X, (double)pointF.Y);
            };
            foreach (TmxChunk chunk in layer.Data.Chunks)
            {
                PolyTree polyTree = LayerClipper.ExecuteClipper(tmxMap, chunk, xfFunc);
                List<List<IntPoint>> list2 = Clipper.ClosedPathsFromPolyTree(polyTree);
                if (list2.Count >= MaxNumberOfSafePaths)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendFormat("Layer '{0}' has a large number of polygon paths ({1}).", layer.Name, list2.Count);
                    stringBuilder.AppendLine("  Importing this layer may be slow in Unity. (Can take an hour or more for +1000 paths.)");
                    stringBuilder.AppendLine("  Check polygon/rectangle objects in Tile Collision Editor in Tiled and use 'Snap to Grid' or 'Snap to Fine Grid'.");
                    stringBuilder.AppendLine("  You want colliders to be set up so they can be merged with colliders on neighboring tiles, reducing path count considerably.");
                    stringBuilder.AppendLine("  In some cases the size of the map may need to be reduced.");
                    Logger.WriteWarning(stringBuilder.ToString());
                }
                if (layer.IsExportingConvexPolygons())
                {
                    AddPolygonCollider2DElements_Convex(obj,polyTree);
                }
                else
                {
                    AddPolygonCollider2DElements_Complex(obj,polyTree);
                }
                AddEdgeCollider2DElements(obj,Clipper.OpenPathsFromPolyTree(polyTree));
                if (list.Count() == 0)
                {
                    return null;
                }
            }
        
            XElement xElement = new XElement("GameObject", new XAttribute("name", "Collision"), list);
            if (string.IsNullOrEmpty(layer.UnityLayerOverrideName) && !string.IsNullOrEmpty(layer.Name))
            {
                xElement.SetAttributeValue("name", "Collision_" + layer.Name);
                xElement.SetAttributeValue("layer", layer.Name);
            }

            return obj;
        }

        private void AddPolygonCollider2DElements_Convex(UnityEngine.GameObject obj, PolyTree solution)
        {
            var ab = LayerClipper.SolutionPolygons_Simple(solution);
           // var cd = obj.AddComponent<PolygonCollider2D>();
          //  int i= 0;
          //  cd.pathCount = ab.Count();
            foreach (PointF[] item2 in ab)
            {
                var cd = obj.AddComponent<PolygonCollider2D>();
                List<UnityEngine.Vector2> v2 = new List<Vector2>();
             foreach(var pt in item2)
                {
                    v2.Add(new Vector2(pt.X * Settings.Scale, pt.Y * Settings.Scale));
                }
                cd.pathCount = 1;
                cd.SetPath(0, v2.ToArray());
            //    string content = string.Join(" ", from pt in item2
            //                                      select $"{pt.X * Settings.Scale},{pt.Y * Settings.Scale}");
             //   XElement content2 = new XElement("Path", content);
             //   XElement item = new XElement("PolygonCollider2D", content2);

              
            }
        }

        private void AddPolygonCollider2DElements_Complex(UnityEngine.GameObject obj,PolyTree solution)
        {
            List<List<IntPoint>> list = Clipper.ClosedPathsFromPolyTree(solution);
           

            if (list.Count != 0)
            {
              
                List<List<UnityEngine.Vector2>> v2 = new List<List<UnityEngine.Vector2>>();
                foreach (List<IntPoint> item3 in list)
                {
                    var v1 = new List<UnityEngine.Vector2>();
                    //string content = string.Join(" ", from pt in item3
                    //                                  select $"{(float)pt.X * Settings.Scale},{(float)pt.Y * Settings.Scale}");
                   foreach(var vv in item3)
                    {
                        v1.Add(new UnityEngine.Vector2(vv.X * Settings.Scale, vv.Y * Settings.Scale));
                    }
                //    XElement item = new XElement("Path", content);
                    v2.Add(v1);
                 
                }
              var iw=  obj.AddComponent<UnityEngine.PolygonCollider2D>();
                List<UnityEngine.Vector2> vaa = new List<UnityEngine.Vector2>();
                iw.pathCount = v2.Count;
                for(int i=0;i<v2.Count;i++)
                {
                    var va = v2[i];
                foreach(var vd in va)
                    {
                        vaa.Add(vd);
                    }
                 //   iw(i, va.ToArray());
                }
                iw.SetPath(0,vaa.ToArray());
              
            }
        }

        private void AddEdgeCollider2DElements(GameObject obj, List<List<IntPoint>> lines)
        {
            if (lines.Count != 0)
            {
                foreach (List<IntPoint> item2 in CombineLineSegments(lines))
                {
                    var cg = obj.AddComponent<EdgeCollider2D>();
                    var list = new List<Vector2>();
                    foreach(var v in item2)
                    {
                        list.Add(new Vector2(v.X * Settings.Scale, v.Y * Settings.Scale));
                    }
                    cg.points = list.ToArray();
                    //string content = string.Join(" ", from pt in item2
                    //                                  select $"{(float)pt.X * Settings.Scale},{(float)pt.Y * Settings.Scale}");
                    //XElement item = new XElement("EdgeCollider2D", new XElement("Points", content));
                    //xmlList.Add(item);
                }
            }
        }

        private List<List<IntPoint>> CombineLineSegments(List<List<IntPoint>> lines)
        {
            PolylineReduction polylineReduction = new PolylineReduction();
            foreach (List<IntPoint> line in lines)
            {
                polylineReduction.AddLine(line);
            }
            return polylineReduction.Reduce();
        }

        public TiledMapExporter(TmxMap tmxMap)
        {
            this.tmxMap = tmxMap;
        }

        public void Export(string exportToTiled2UnityPath)
        {
            //if (string.IsNullOrEmpty(exportToTiled2UnityPath))
            //{
            //    throw new TmxException("Unity project export path is invalid or not set.");
            //}
            string exportedFilename = tmxMap.GetExportedFilename();
            Logger.WriteInfo("Compiling tiled2unity file: {0}", exportedFilename);
            List<XElement> content = CreateImportFilesElements(exportToTiled2UnityPath);
            List<XElement> content2 = CreateAssignMaterialsElements();
            Logger.WriteVerbose("Gathering prefab data ...");
            var content3 = CreatePrefabElement();
            Logger.WriteVerbose("Writing as Xml ...");
            string version = Info.GetVersion();
           
            
          
            
        }

        private static string GetTiled2UnityVersionInProject(string path)
        {
            try
            {
                return XDocument.Load(path).Element("Tiled2UnityImporter").Element("Header")
                    .Attribute("version")
                    .Value;
            }
            catch (Exception ex)
            {
                Logger.WriteWarning("Couldn't get Tiled2Unity version from '{0}'\n{1}", path, ex.Message);
                return "tiled2unity.get.version.fail";
            }
        }

        public static PointF PointFToUnityVector_NoScale(PointF pt)
        {
            return new PointF(pt.X, (pt.Y == 0f) ? 0f : (0f - pt.Y));
        }

        public static PointF PointFToUnityVector(float x, float y)
        {
            return PointFToUnityVector(new PointF(x, y));
        }

        public static PointF PointFToUnityVector(PointF pt)
        {
            PointF pointF = pt;
            pointF.X *= Settings.Scale;
            pointF.Y *= Settings.Scale;
            return new PointF(pointF.X, (pointF.Y == 0f) ? 0f : (0f - pointF.Y));
        }

        public static PointF PointFToObjVertex(PointF pt)
        {
            PointF pointF = pt;
            pointF.X *= Settings.Scale;
            pointF.Y *= Settings.Scale;
            return new PointF((pointF.X == 0f) ? 0f : (0f - pointF.X), (pointF.Y == 0f) ? 0f : (0f - pointF.Y));
        }

        public static PointF PointToTextureCoordinate(PointF pt, Size imageSize)
        {
            float x = pt.X / (float)imageSize.Width;
            float num = pt.Y / (float)imageSize.Height;
            return new PointF(x, 1f - num);
        }

        public static float CalculateFaceDepth(float position_y, float mapHeight)
        {
            float num = position_y / mapHeight * -1f;
            if (num != -0f)
            {
                return num;
            }
            return 0f;
        }

        public static float CalculateLayerDepth(TmxLayerNode layer)
        {
            if (!Settings.DepthBufferEnabled)
            {
                return 0f;
            }
            float num = (float)layer.ParentMap.TileHeight / (float)layer.ParentMap.MapSizeInPixels.Height;
            float num2 = (float)layer.DepthBufferIndex * num * -1f;
            float num3 = layer.Offset.Y / (float)layer.ParentMap.TileHeight;
            num2 -= num3 * num;
            if (num2 != -0f)
            {
                return num2;
            }
            return 0f;
        }

        private string FileToBase64String(string path)
        {
            return Convert.ToBase64String(File.ReadAllBytes(path));
        }

        private string FileToCompressedBase64String(string path)
        {
            using (FileStream fileStream = File.OpenRead(path))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (GZipStream destination = new GZipStream(memoryStream, CompressionMode.Compress))
                    {
                        fileStream.CopyTo(destination);
                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }

        List<UnityEngine.Mesh> meshes;
        List<UnityEngine.Material> materials;
        private List<XElement> CreateImportFilesElements(string exportToUnityProjectPath)
        {
            List<XElement> list = new List<XElement>();

            meshes = new List<UnityEngine.Mesh>();
            materials = new List<UnityEngine.Material>();
            foreach (var item in EnumerateImportMeshElements())
            {
                meshes.Add(item);
            //    list.Add(item);
            }
            foreach (var item2 in EnumerateTextureElements(exportToUnityProjectPath))
            {
                materials.Add(item2);
           //     list.Add(item2);
            }
            return list;
        }

        private IEnumerable<UnityEngine.Mesh> EnumerateImportMeshElements()
        {
            foreach (Tuple<string, UnityEngine.Mesh> item3 in EnumerateWavefrontData())
            {
                string item = item3.Item1;
                var item2 = item3.Item2;
                string value = $"{item}.obj";

                item2.name = item;

                yield return item2;//new XElement("ImportMesh", new XAttribute("filename", value), item2.ToString().ToBase64());
            }
        }

        private IEnumerable<UnityEngine.Material> EnumerateTextureElements(string exportToUnityProjectPath)
        {
            IEnumerable<TmxImage> collection = from layer in tmxMap.EnumerateTileLayers()
                                               where layer.Visible
                                               from chunk in layer.Data.Chunks
                                               from rawTileId in chunk.TileIds
                                               where rawTileId != 0
                                               let tileId = TmxMath.GetTileIdWithoutFlags(rawTileId)
                                               let tile = tmxMap.Tiles[tileId]
                                               select tile.TmxImage;
            IEnumerable<TmxImage> collection2 = from layer in tmxMap.EnumerateTileLayers()
                                                where layer.Visible
                                                from chunk in layer.Data.Chunks
                                                from rawTileId in chunk.TileIds
                                                where rawTileId != 0
                                                let tileId = TmxMath.GetTileIdWithoutFlags(rawTileId)
                                                let tile = tmxMap.Tiles[tileId]
                                                from rawFrame in tile.Animation.Frames
                                                let frameId = TmxMath.GetTileIdWithoutFlags(rawFrame.GlobalTileId)
                                                let frame = tmxMap.Tiles[frameId]
                                                select frame.TmxImage;
            IEnumerable<TmxImage> collection3 = from objectGroup in tmxMap.EnumerateObjectLayers()
                                                where objectGroup.Visible
                                                from tmxObject in objectGroup.Objects
                                                where tmxObject.Visible
                                                where tmxObject is TmxObjectTile
                                                let tmxTileObject = tmxObject as TmxObjectTile
                                                from mesh in tmxTileObject.Tile.Meshes
                                                select mesh.TmxImage;
            List<TmxImage> list = new List<TmxImage>();
            list.AddRange(collection);
            list.AddRange(collection2);
            list.AddRange(collection3);
            TmxImageComparer comparer = new TmxImageComparer();
            list = list.Distinct(comparer).ToList();
            List<TmxImage>.Enumerator enumerator = list.GetEnumerator();

            string shaderName = "Tiled2Unity/";

            try
            {
                while (enumerator.MoveNext())
                {
                    TmxImage image = enumerator.Current;
                   
                    {
                      //  XElement xElement2 = new XElement("ImportTexture");
                        Logger.WriteInfo("ImportTexture : will import '{0}' to {1}", image.AbsolutePath, Path.Combine(exportToUnityProjectPath, "Textures"));
                        if (!string.IsNullOrEmpty(image.TransparentColor))
                        {
                         
                          //  xElement2.SetAttributeValue("alphaColorKey", image.TransparentColor);
                            shaderName += " Color Key";
                        }
                        if (Settings.DepthBufferEnabled)
                        {
                            shaderName += "Depth";
                            //   xElement2.SetAttributeValue("usesDepthShaders", true);
                        }
                        else
                        {
                            shaderName += "Default";
                        }
                        //if (tmxMap.IsResource)
                        //{
                        //    xElement2.SetAttributeValue("isResource", true);
                        //}
                        shaderName += " (Instanced)";
                        string value = image.ImageName + Path.GetExtension(image.AbsolutePath);
                        var mat = new UnityEngine.Material(UnityEngine.Shader.Find(shaderName));
                        mat.name = Path.GetFileNameWithoutExtension(image.AbsolutePath);
                        mat.SetTexture("_MainTex", image.ImageBitmap);
                        //   xElement2.Add(new XAttribute("filename", value), FileToBase64String(image.AbsolutePath));
                        yield return mat;
                    }
                }
            }
            finally
            {
                ((IDisposable)enumerator).Dispose();
            }
            enumerator = default(List<TmxImage>.Enumerator);
        }

        private string GetUnityAssetsPath(string path)
        {
            string directoryName = Path.GetDirectoryName(path);
            while (!string.IsNullOrEmpty(directoryName) && Directory.Exists(directoryName))
            {
                if (string.Compare(Path.GetFileName(directoryName), "Assets", true) == 0)
                {
                    return directoryName;
                }
                directoryName = Path.GetDirectoryName(directoryName);
            }
            return Path.GetDirectoryName(path);
        }

        private IEnumerable<Tuple<string, UnityEngine.Mesh>> EnumerateWavefrontData()
        {
            Logger.WriteVerbose("Enumerate map layers for mesh-build.");
            List<TmxMesh>.Enumerator enumerator2;
            foreach (TmxLayer item in tmxMap.EnumerateTileLayers())
            {
                if (item.Visible && item.Ignore != TmxLayerNode.IgnoreSettings.Visual)
                {
                    enumerator2 = item.Meshes.GetEnumerator();
                    try
                    {
                        while (enumerator2.MoveNext())
                        {
                            TmxMesh current = enumerator2.Current;
                            IEnumerable<int> verticalRange = (tmxMap.DrawOrderVertical == 1) ? Enumerable.Range(0, current.Height) : Enumerable.Range(0, current.Height).Reverse();
                            IEnumerable<int> horizontalRange = (tmxMap.DrawOrderHorizontal == 1) ? Enumerable.Range(0, current.Width) : Enumerable.Range(0, current.Width).Reverse();
                            yield return Tuple.Create(current.UniqueMeshName, BuildWavefrontStringForLayerMesh(item, current, horizontalRange, verticalRange));
                        }
                    }
                    finally
                    {
                        ((IDisposable)enumerator2).Dispose();
                    }
                    enumerator2 = default(List<TmxMesh>.Enumerator);
                }
            }
            Logger.WriteVerbose("Finished enumeration.");
            Logger.WriteVerbose("Enumerate tile objects for mesh-build.");
            enumerator2 = tmxMap.GetUniqueListOfVisibleObjectTileMeshes().GetEnumerator();
            try
            {
                while (enumerator2.MoveNext())
                {
                    TmxMesh current2 = enumerator2.Current;
                    yield return Tuple.Create(current2.UniqueMeshName, BuildWavefrontStringForTileObjectMesh(current2));
                }
            }
            finally
            {
                ((IDisposable)enumerator2).Dispose();
            }
            enumerator2 = default(List<TmxMesh>.Enumerator);
            Logger.WriteVerbose("Finished enumeration.");
        }

        private UnityEngine.Mesh BuildWavefrontStringForLayerMesh(TmxLayer layer, TmxMesh mesh, IEnumerable<int> horizontalRange, IEnumerable<int> verticalRange)
        {
            Logger.WriteVerbose("Building mesh obj file for '{0}'", mesh.UniqueMeshName);
            GenericListDatabase<Vertex3> genericListDatabase = new GenericListDatabase<Vertex3>();
            HashIndexOf<PointF> hashIndexOf = new HashIndexOf<PointF>();
            StringBuilder stringBuilder = new StringBuilder();
            var m = new UnityEngine.Mesh();
            m.name = mesh.UniqueMeshName;

            List<UnityEngine.Vector3> v3 = new List<UnityEngine.Vector3>();
            List<UnityEngine.Vector3> ns = new List<UnityEngine.Vector3>();
            List<UnityEngine.Vector2> v2 = new List<UnityEngine.Vector2>();
            List<int> iw = new List<int>();
            foreach (int item in verticalRange)
            {
                foreach (int item2 in horizontalRange)
                {
                    int tileIndex = mesh.GetTileIndex(item2, item);
                    uint tileIdAt = mesh.GetTileIdAt(tileIndex);
                    if (tileIdAt != 0)
                    {
                        TmxTile tmxTile = tmxMap.Tiles[TmxMath.GetTileIdWithoutFlags(tileIdAt)];
                        Point mapPositionAt = tmxMap.GetMapPositionAt(item2, item, tmxTile);
                        PointF[] vertices = CalculateFaceVertices(mapPositionAt, tmxTile.TileSize);
                        float depth_z = 0f;
                        if (Settings.DepthBufferEnabled)
                        {
                            depth_z = CalculateFaceDepth((float)(mapPositionAt.Y + tmxMap.TileHeight), (float)tmxMap.MapSizeInPixels.Height);
                        }
                        FaceVertices faceVertices = default(FaceVertices);
                        faceVertices.Vertices = vertices;
                        faceVertices.Depth_z = depth_z;
                        FaceVertices faceVertices2 = faceVertices;
                        bool flipDiagonal = TmxMath.IsTileFlippedDiagonally(tileIdAt);
                        bool flipHorizontal = TmxMath.IsTileFlippedHorizontally(tileIdAt);
                        bool flipVertical = TmxMath.IsTileFlippedVertically(tileIdAt);
                        PointF[] array = CalculateFaceTextureCoordinates(tmxTile, flipDiagonal, flipHorizontal, flipVertical);
                        //string text = $"{genericListDatabase.AddToDatabase(faceVertices2.V0) + 1}/{hashIndexOf.Add(array[0]) + 1}/1";
                        //string text2 = $"{genericListDatabase.AddToDatabase(faceVertices2.V1) + 1}/{hashIndexOf.Add(array[1]) + 1}/1";
                        //string text3 = $"{genericListDatabase.AddToDatabase(faceVertices2.V2) + 1}/{hashIndexOf.Add(array[2]) + 1}/1";
                        //string text4 = $"{genericListDatabase.AddToDatabase(faceVertices2.V3) + 1}/{hashIndexOf.Add(array[3]) + 1}/1";
                        v3.Add(new UnityEngine.Vector3(-(faceVertices2.V0.X), faceVertices2.V0.Y, faceVertices2.V0.Z));
                        v3.Add(new UnityEngine.Vector3(-(faceVertices2.V1.X), faceVertices2.V1.Y, faceVertices2.V1.Z));
                        v3.Add(new UnityEngine.Vector3(-(faceVertices2.V2.X), faceVertices2.V2.Y, faceVertices2.V2.Z));
                    
                        v3.Add(new UnityEngine.Vector3(-(faceVertices2.V3.X), faceVertices2.V3.Y, faceVertices2.V3.Z));


                        iw.Add(genericListDatabase.AddToDatabase(faceVertices2.V0));
                        iw.Add(genericListDatabase.AddToDatabase(faceVertices2.V1));
                        iw.Add(genericListDatabase.AddToDatabase(faceVertices2.V2));
                        iw.Add(genericListDatabase.AddToDatabase(faceVertices2.V3));

                     
                        v2.Add(new UnityEngine.Vector2(array[0].X, array[0].Y));
                        v2.Add(new UnityEngine.Vector2(array[1].X, array[1].Y));
                        v2.Add(new UnityEngine.Vector2(array[2].X, array[2].Y));
                        v2.Add(new UnityEngine.Vector2(array[3].X, array[3].Y));
                        ns.Add(new UnityEngine.Vector3(0, 0, -1));
                        ns.Add(new UnityEngine.Vector3(0, 0, -1));
                        ns.Add(new UnityEngine.Vector3(0, 0, -1));
                        ns.Add(new UnityEngine.Vector3(0, 0, -1));
                    }
                }
            }
            m.vertices = v3.ToArray();
            m.uv = v2.ToArray();
            m.normals = ns.ToArray();
            m.SetIndices(iw.ToArray(), UnityEngine.MeshTopology.Quads, 0);
            m.SetVertices(v3);
            var cou = genericListDatabase;
            UnityEngine.Debug.LogError(cou.List.Count + "wr ");
            UnityEngine.Debug.LogError(v3.Count + "wr2 ");
            return m;//CreateWavefrontWriter(mesh, genericListDatabase, hashIndexOf, stringBuilder);
        }

        private UnityEngine.Mesh BuildWavefrontStringForTileObjectMesh(TmxMesh mesh)
        {

            Logger.WriteVerbose("Building mesh obj file for tile: '{0}.obj'", mesh.UniqueMeshName);
            GenericListDatabase<Vertex3> genericListDatabase = new GenericListDatabase<Vertex3>();
            HashIndexOf<PointF> hashIndexOf = new HashIndexOf<PointF>();
            StringBuilder stringBuilder = new StringBuilder();
            TmxTile tmxTile = tmxMap.Tiles[mesh.TileIds[0]];
            PointF[] vertices = CalculateFaceVertices_TileObject(tmxTile.TileSize, tmxTile.Offset);
            PointF[] array = CalculateFaceTextureCoordinates(tmxTile, false, false, false);
            FaceVertices faceVertices = default(FaceVertices);
            faceVertices.Vertices = vertices;
            faceVertices.Depth_z = 0f;
            FaceVertices faceVertices2 = faceVertices;
            var a1 = genericListDatabase.AddToDatabase(faceVertices2.V0);
            var a2 = genericListDatabase.AddToDatabase(faceVertices2.V1);
            var a3 = genericListDatabase.AddToDatabase(faceVertices2.V2);
            var a4 = genericListDatabase.AddToDatabase(faceVertices2.V3);

            

            var umesh = new UnityEngine.Mesh();
            umesh.name = mesh.UniqueMeshName;

            List<UnityEngine.Vector3> v3 = new List<UnityEngine.Vector3>();
            List<UnityEngine.Vector2> v2 = new List<UnityEngine.Vector2>();
            List<UnityEngine.Vector3> ns = new List<UnityEngine.Vector3>();
            List<int> vs = new List<int>();
            int i = 0;
            foreach (var  v in vertices)
            {
             //   v3.Add(new UnityEngine.Vector3(v.X, v.Y, 0f));
                ns.Add(new UnityEngine.Vector3(0, 0, -1));
              //  vs.Add(i);
                i++;

            }
            v3.Add(new UnityEngine.Vector3(-(faceVertices2.V0.X ), faceVertices.V0.Y, faceVertices.V0.Z));
            v3.Add(new UnityEngine.Vector3(-(faceVertices2.V1.X), faceVertices.V1.Y, faceVertices.V1.Z));
            v3.Add(new UnityEngine.Vector3(-(faceVertices2.V2.X), faceVertices.V2.Y, faceVertices.V2.Z));
            v3.Add(new UnityEngine.Vector3(-(faceVertices2.V3.X), faceVertices.V3.Y, faceVertices.V3.Z));

            foreach (var v in array)
            {
                v2.Add(new UnityEngine.Vector2(v.X, v.Y));
            }
            umesh.vertices = v3.ToArray();
            umesh.SetIndices(new int[] { a1,a2,a3,a4}, UnityEngine.MeshTopology.Quads, 0);
            umesh.normals = ns.ToArray();
            umesh.uv = v2.ToArray();


            return umesh;// CreateWavefrontWriter(mesh, genericListDatabase, hashIndexOf, stringBuilder);
        }

        private StringWriter CreateWavefrontWriter(TmxMesh mesh, GenericListDatabase<Vertex3> vertexDatabase, HashIndexOf<PointF> uvDatabase, StringBuilder faces)
        {
            StringWriter stringWriter = new StringWriter();
            stringWriter.WriteLine("# Tiled2Unity generated file. Do not modify by hand.");
            stringWriter.WriteLine("# Wavefront file for '{0}.obj'", mesh.UniqueMeshName);
            stringWriter.WriteLine();
            stringWriter.WriteLine("# Vertices (Count = {0})", vertexDatabase.List.Count());
            foreach (Vertex3 item in vertexDatabase.List)
            {
                stringWriter.WriteLine("v {0} {1} {2}", item.X, item.Y, item.Z);
            }
            stringWriter.WriteLine();
            stringWriter.WriteLine("# Texture cooridinates (Count = {0})", uvDatabase.List.Count());
            foreach (PointF item2 in uvDatabase.List)
            {
                stringWriter.WriteLine("vt {0} {1}", item2.X, item2.Y);
            }
            stringWriter.WriteLine();
            stringWriter.WriteLine("# Normal");
            stringWriter.WriteLine("vn 0 0 -1");
            stringWriter.WriteLine();
            stringWriter.WriteLine("# Mesh description");
            stringWriter.WriteLine("g {0}", mesh.UniqueMeshName);
            stringWriter.WriteLine();
            stringWriter.WriteLine("# Faces");
            stringWriter.WriteLine(faces.ToString());
            return stringWriter;
        }

        private PointF[] CalculateFaceVertices(Point mapLocation, Size tileSize)
        {
            PointF pt = mapLocation;
            PointF pt2 = PointF.Add(mapLocation, new Size(tileSize.Width, 0));
            PointF pt3 = PointF.Add(mapLocation, tileSize);
            PointF pt4 = PointF.Add(mapLocation, new Size(0, tileSize.Height));
            PointF[] obj = new PointF[4]
            {
                    default(PointF),
                    default(PointF),
                    default(PointF),
                    PointFToObjVertex(pt)
            };
            obj[2] = PointFToObjVertex(pt2);
            obj[1] = PointFToObjVertex(pt3);
            obj[0] = PointFToObjVertex(pt4);
            return obj;
        }

        private PointF[] CalculateFaceVertices_TileObject(Size tileSize, PointF offset)
        {
            PointF empty;
            PointF pt = empty = PointF.Empty;
            PointF a = PointF.Add(pt, new Size(tileSize.Width, 0));
            PointF a2 = PointF.Add(pt, tileSize);
            PointF a3 = PointF.Add(pt, new Size(0, tileSize.Height));
            empty = TmxMath.AddPoints(empty, offset);
            a = TmxMath.AddPoints(a, offset);
            a2 = TmxMath.AddPoints(a2, offset);
            a3 = TmxMath.AddPoints(a3, offset);
            PointF[] obj = new PointF[4]
            {
                    default(PointF),
                    default(PointF),
                    default(PointF),
                    PointFToObjVertex(empty)
            };
            obj[2] = PointFToObjVertex(a);
            obj[1] = PointFToObjVertex(a2);
            obj[0] = PointFToObjVertex(a3);
            return obj;
        }

        private PointF[] CalculateFaceTextureCoordinates(TmxTile tmxTile, bool flipDiagonal, bool flipHorizontal, bool flipVertical)
        {
            Point locationOnSource = tmxTile.LocationOnSource;
            Size tileSize = tmxTile.TileSize;
            Size size = tmxTile.TmxImage.Size;
            PointF[] array = new PointF[4]
            {
                    locationOnSource,
                    PointF.Add(locationOnSource, new Size(tileSize.Width, 0)),
                    PointF.Add(locationOnSource, tileSize),
                    PointF.Add(locationOnSource, new Size(0, tileSize.Height))
            };
            PointF origin = new PointF((float)tileSize.Width * 0.5f, (float)tileSize.Height * 0.5f);
            origin.X += (float)locationOnSource.X;
            origin.Y += (float)locationOnSource.Y;
            TmxMath.TransformPoints_DiagFirst(array, origin, flipDiagonal, flipHorizontal, flipVertical);
            float s = 0f;
            PointF[] array2 = new PointF[4];
            if (Settings.TexelBias > 0f)
            {
                s = 1f / Settings.TexelBias;
                array2[0].X += 1f;
                array2[0].Y += 1f;
                array2[1].X -= 1f;
                array2[1].Y += 1f;
                array2[2].X -= 1f;
                array2[2].Y -= 1f;
                array2[3].X += 1f;
                array2[3].Y -= 1f;
            }
            TmxMath.TransformPoints_DiagFirst(array2, PointF.Empty, flipDiagonal, flipHorizontal, flipVertical);
            PointF[] obj = new PointF[4]
            {
                    default(PointF),
                    default(PointF),
                    default(PointF),
                    TmxMath.AddPoints(PointToTextureCoordinate(array[0], size), TmxMath.ScalePoint(array2[0].X, 0f - array2[0].Y, s))
            };
            obj[2] = TmxMath.AddPoints(PointToTextureCoordinate(array[1], size), TmxMath.ScalePoint(array2[1].X, 0f - array2[1].Y, s));
            obj[1] = TmxMath.AddPoints(PointToTextureCoordinate(array[2], size), TmxMath.ScalePoint(array2[2].X, 0f - array2[2].Y, s));
            obj[0] = TmxMath.AddPoints(PointToTextureCoordinate(array[3], size), TmxMath.ScalePoint(array2[3].X, 0f - array2[3].Y, s));
            return obj;
        }

        private UnityEngine.GameObject CreatePrefabElement()
        {
            Size mapSizeInPixels = tmxMap.MapSizeInPixels;
            var xElement = new UnityEngine.GameObject(tmxMap.Name);
       var a=    xElement.AddComponent<TiledMap>();
            a.Orientation = tmxMap.Orientation;
            a.StaggerAxis = tmxMap.StaggerAxis;
            a.StaggerIndex = tmxMap.StaggerIndex;
            a.NumLayers = tmxMap.EnumerateTileLayers().Count();
            a.NumTilesWide = tmxMap.Width;
            a.NumTilesHigh = tmxMap.Height;
            a.TileWidth = tmxMap.TileWidth;
            a.TileHeight = tmxMap.TileHeight;
            a.ExportScale = Settings.Scale;
            a.MapHeightInPixels = mapSizeInPixels.Height;
            a.MapWidthInPixels = mapSizeInPixels.Width;
            a.BackgroundColor = tmxMap.BackgroundColor;
            a.HexSideLength = tmxMap.HexSideLength;
           
            //    XElement xElement = new XElement("Prefab");
        //    xElement.SetAttributeValue("name", tmxMap.Name);
        //    xElement.SetAttributeValue("orientation", tmxMap.Orientation.ToString());
        //    xElement.SetAttributeValue("staggerAxis", tmxMap.StaggerAxis.ToString());
        //    xElement.SetAttributeValue("staggerIndex", tmxMap.StaggerIndex.ToString());
        //    xElement.SetAttributeValue("hexSideLength", tmxMap.HexSideLength);
        //    xElement.SetAttributeValue("numLayers", tmxMap.EnumerateTileLayers().Count());
        //    xElement.SetAttributeValue("numTilesWide", tmxMap.Width);
        //    xElement.SetAttributeValue("numTilesHigh", tmxMap.Height);
        //    xElement.SetAttributeValue("tileWidth", tmxMap.TileWidth);
        //    xElement.SetAttributeValue("tileHeight", tmxMap.TileHeight);
        //    xElement.SetAttributeValue("exportScale", Settings.Scale);
        //    xElement.SetAttributeValue("mapWidthInPixels", mapSizeInPixels.Width);
        //    xElement.SetAttributeValue("mapHeightInPixels", mapSizeInPixels.Height);
        ////    xElement.SetAttributeValue("backgroundColor", "#" + tmxMap.BackgroundColor.ToArgb().ToString("x8").Substring(2));
        //    AssignUnityProperties(tmxMap, xElement, PrefabContext.Root);
        //    AssignTiledProperties(tmxMap, xElement);
            foreach (TmxLayerNode layerNode in tmxMap.LayerNodes)
            {
                AddLayerNodeToElement(layerNode, xElement);
            }
            return xElement;
        }

        private void AddLayerNodeToElement(TmxLayerNode node, UnityEngine.GameObject xml)
        {
            if (node.Visible)
            {
                if (node is TmxGroupLayer)
                {
                    AddGroupLayerToElement(node as TmxGroupLayer, xml);
                }
                else if (node is TmxLayer)
                {
                    AddTileLayerToElement(node as TmxLayer, xml);
                }
                else if (node is TmxObjectGroup)
                {
                    AddObjectLayerToElement(node as TmxObjectGroup, xml);
                }
            }
        }

        private void AddGroupLayerToElement(TmxGroupLayer groupLayer, UnityEngine.GameObject xmlRoot)
        {
            var xElement = new UnityEngine.GameObject(groupLayer.Name);
           // xElement.SetAttributeValue("name", groupLayer.Name);
            PointF pointF = PointFToUnityVector(groupLayer.Offset);
            float num = CalculateLayerDepth(groupLayer);

            xElement.transform.position = new UnityEngine.Vector3(pointF.X, pointF.Y, num);
            //   xElement.SetAttributeValue("x", pointF.X);
            //  xElement.SetAttributeValue("y", pointF.Y);
            //  xElement.SetAttributeValue("z", num);
           var a= xElement.AddComponent<GroupLayer>();
            a.Offset = new UnityEngine.Vector2(groupLayer.Offset.X, groupLayer.Offset.Y);
           
            
          //  XElement content = new XElement("GroupLayer", new XAttribute("offsetX", groupLayer.Offset.X), new XAttribute("offsetY", groupLayer.Offset.Y));
            //xElement.Add(content);
            foreach (TmxLayerNode layerNode in groupLayer.LayerNodes)
            {
                AddLayerNodeToElement(layerNode, xElement);
            }
            xElement.transform.SetParent(xmlRoot.transform);
            
         //   xmlRoot.Add(xElement);
        }

        private void AddTileLayerToElement(TmxLayer tileLayer, UnityEngine.GameObject xmlRoot)
        {
            

            var xa = new UnityEngine.GameObject(tileLayer.Name);
          //  xElement.SetAttributeValue("name", tileLayer.Name);
            PointF pointF = PointFToUnityVector(tileLayer.Offset);
            float num = CalculateLayerDepth(tileLayer);
            //  xElement.SetAttributeValue("x", pointF.X);
            //  xElement.SetAttributeValue("y", pointF.Y);
            //  xElement.SetAttributeValue("z", num);
            xa.transform.position = new UnityEngine.Vector3(pointF.X, pointF.Y, num);
            var a = xa.AddComponent<TileLayer>();
            a.Offset = new UnityEngine.Vector2(tileLayer.Offset.X, tileLayer.Offset.Y);
          //  XElement content = new XElement("TileLayer", new XAttribute("offsetX", tileLayer.Offset.X), new XAttribute("offsetY", tileLayer.Offset.Y));
           // xElement.Add(content);
            var ct = xa.AddComponent<TileLayer>();


            if (tileLayer.Ignore != TmxLayerNode.IgnoreSettings.Visual)
            {
              var content2 = CreateMeshElementsForLayer(tileLayer);
               foreach(var v in content2)
                {
                    v.transform.SetParent(xa.transform);
                }
              //  xElement.Add(content2);
            }
            if (tileLayer.Ignore != TmxLayerNode.IgnoreSettings.Collision)
            {
                foreach (TmxLayer collisionLayer in tileLayer.CollisionLayers)
                {
                    var content3 = CreateCollisionElementForLayer(collisionLayer);
                    content3.transform.SetParent(xa.transform);
                    //  xElement.Add(content3);
                }
            }
          //  AssignUnityProperties(tileLayer, xElement, PrefabContext.TiledLayer);
         //   AssignTiledProperties(tileLayer, xElement);
            xa.transform.SetParent(xmlRoot.transform);
        }

        private void AddObjectLayerToElement(TmxObjectGroup objectLayer, UnityEngine.GameObject xmlRoot)
        {
            var go = new UnityEngine.GameObject(objectLayer.Name);
           
           
         //   XElement xElement = new XElement("GameObject");
           // xElement.SetAttributeValue("name", objectLayer.Name);
            PointF pointF = PointFToUnityVector(objectLayer.Offset);
            float num = CalculateLayerDepth(objectLayer);
            go.transform.position = new UnityEngine.Vector3(pointF.X, pointF.Y, num);

            // xElement.SetAttributeValue("x", pointF.X);
            // xElement.SetAttributeValue("y", pointF.Y);
            // xElement.SetAttributeValue("z", num);
            //  XElement content = new XElement("ObjectLayer", new XAttribute("offsetX", objectLayer.Offset.X), new XAttribute("offsetY", objectLayer.Offset.Y), new XAttribute("color", "#" + objectLayer.Color..ToArgb().ToString("x8")));
            // xElement.Add(content);
            go.transform.position = new UnityEngine.Vector3(pointF.X, pointF.Y, num);

          var objlayer=  go.AddComponent<ObjectLayer>();
            objlayer.Offset = new UnityEngine.Vector2(objectLayer.Offset.X, objectLayer.Offset.Y);
           // go.AddComponent

          //  AssignUnityProperties(objectLayer, xElement, PrefabContext.ObjectLayer);
          //  AssignTiledProperties(objectLayer, xElement);
            var list = CreateObjectElementList(objectLayer);


            if (list.Count() > 0)
            {
                foreach(var g in list)
                {
                    g.transform.SetParent(go.transform);
                }
      //          xElement.Add(list);
            }
        //    xmlRoot.Add(xElement);
        }

        private List<UnityEngine.GameObject> CreateObjectElementList(TmxObjectGroup objectGroup)
        {
            List<UnityEngine.GameObject> list = new List<UnityEngine.GameObject>();
            foreach (TmxObject @object in objectGroup.Objects)
            {
                var xElement = new UnityEngine.GameObject(@object.GetNonEmptyName());// new XElement("GameObject", new XAttribute("name", @object.GetNonEmptyName()));
                PointF pointF = PointFToUnityVector(TmxMath.ObjectPointFToMapSpace(tmxMap, @object.Position));
                //xElement.SetAttributeValue("x", pointF.X);
                //xElement.SetAttributeValue("y", pointF.Y);
                //xElement.SetAttributeValue("rotation", @object.Rotation);
                xElement.transform.position = new UnityEngine.Vector3(pointF.X, pointF.Y);
                Vector3 localRotation = new Vector3();
                localRotation.z = -@object.Rotation;
                xElement.transform.eulerAngles=(localRotation);
             //   AssignUnityProperties(@object, xElement, PrefabContext.Object);
              //  AssignTiledProperties(@object, xElement);
                if (@object.GetType() != typeof(TmxObjectTile) && string.IsNullOrEmpty(objectGroup.UnityLayerOverrideName))
                {
                    xElement.layer = UnityEngine.LayerMask.NameToLayer(@object.Type);
                //    xElement.SetAttributeValue("layer", @object.Type);
                }
                UnityEngine.Collider2D xElement2 = null;

                var xElement3 = xElement.AddComponent<TmxObject2>();// new XElement("TmxObjectComponent", new XAttribute("tmx-object-id", @object.Id), new XAttribute("tmx-object-name", @object.Name), new XAttribute("tmx-object-type", @object.Type), new XAttribute("tmx-object-x", @object.Position.X), new XAttribute("tmx-object-y", @object.Position.Y), new XAttribute("tmx-object-width", @object.Size.Width), new XAttribute("tmx-object-height", @object.Size.Height), new XAttribute("tmx-object-rotation", @object.Rotation));
                xElement3.TmxId = @object.Id;
                xElement3.TmxName = @object.Name;
                xElement3.TmxType = @object.Type;
                xElement3.TmxPosition = new UnityEngine.Vector2(@object.Position.X, @object.Position.Y);
                xElement3.TmxSize = new UnityEngine.Vector2(@object.Size.Width, @object.Size.Height);
                xElement3.TmxRotation = @object.Rotation;
                bool flag = objectGroup.Ignore == TmxLayerNode.IgnoreSettings.Collision;
                if (!flag && @object.GetType() == typeof(TmxObjectRectangle))
                {
                    if (tmxMap.Orientation == TmxMap.MapOrientation.Isometric)
                    {
                        TmxObjectPolygon tmxPolygon = TmxObjectPolygon.FromRectangle(tmxMap, @object as TmxObjectRectangle);
                        xElement2 = CreatePolygonColliderElement(xElement,tmxPolygon);
                    }
                    else
                    {
                        xElement2 = CreateBoxColliderElement(xElement,@object as TmxObjectRectangle);
                    }
                    xElement3.TmxName = "RectangleObjectComponent";
                }
                else if (!flag && @object.GetType() == typeof(TmxObjectEllipse))
                {
                    xElement2 = CreateCircleColliderElement(xElement,@object as TmxObjectEllipse, objectGroup.Name);
                    xElement3.TmxName = "CircleObjectComponent";
                }
                else if (!flag && @object.GetType() == typeof(TmxObjectPolygon))
                {
                    xElement2 = CreatePolygonColliderElement(xElement,@object as TmxObjectPolygon);
                    xElement3.TmxName = "PolygonObjectComponent";
                }
                else if (!flag && @object.GetType() == typeof(TmxObjectPolyline))
                {
                    xElement2 = CreateEdgeColliderElement(xElement,@object as TmxObjectPolyline);
                    xElement3.TmxName = "PolylineObjectComponent";
                }
                else if (@object.GetType() == typeof(TmxObjectTile))
                {
                    TmxObjectTile tmxObjectTile = @object as TmxObjectTile;
                    if (Settings.DepthBufferEnabled)
                    {
                        float num = CalculateFaceDepth(tmxObjectTile.Position.Y, (float)tmxMap.MapSizeInPixels.Height);
                        var pw = xElement.transform.position;
                        xElement.transform.position=new UnityEngine.Vector3(pw.x,pw.y,num);
                        // xElement.SetAttributeValue("z", num);
                    }
                    AddTileObjectElements(tmxObjectTile, xElement, null);
                }
                else
                {
                    Logger.WriteInfo("Object '{0}' has been added for use with custom importers", @object);
                }
                if (xElement2 != null)
                {
                    //xElement.Add(xElement3);
                    //xElement.Add(xElement2);
                }else
                {
                    UnityEngine.Object.Destroy(xElement3);
                    UnityEngine.Object.Destroy(xElement2);
                }
                list.Add(xElement);
            }
            return list;
        }

        private List<UnityEngine.GameObject> CreateMeshElementsForLayer(TmxLayer layer)
        {
      //      List<XElement> list = new List<XElement>();
            List<UnityEngine.GameObject> gameObjects = new List<UnityEngine.GameObject>();
            foreach (TmxMesh mesh in layer.Meshes)
            {
              //  XElement xElement = new XElement("GameObject", new XAttribute("name", mesh.ObjectName), new XAttribute("x", (float)(mesh.X * layer.ParentMap.TileWidth) * Settings.Scale), new XAttribute("y", (float)(mesh.Y * layer.ParentMap.TileHeight) * Settings.Scale), new XAttribute("mesh", mesh.UniqueMeshName), new XAttribute("sortingLayerName", layer.GetSortingLayerName()), new XAttribute("sortingOrder", layer.GetSortingOrder()), new XAttribute("opacity", layer.GetRecursiveOpacity()));
                var go = new UnityEngine.GameObject(mesh.ObjectName);
                gameObjects.Add(go);
var a=                go.GetOrAddComponent<UnityEngine.MeshFilter>();
                var b = go.GetOrAddComponent<UnityEngine.MeshRenderer>();

                a.mesh = meshes.Find(x => x.name == mesh.UniqueMeshName);
                b.material = materials.Find(x => x.name ==mtm[mesh.UniqueMeshName]);
                //  list.Add(xElement);
                if (mesh.FullAnimationDurationMs > 0)
                {
                    var ta = go.AddComponent<TileAnimator>();
                    ta.StartTime = mesh.StartTimeMs *0.001f;
                    ta.Duration = mesh.DurationMs *0.001f;
                    ta.TotalAnimationTime = mesh.FullAnimationDurationMs* 0.001f;

                  //  XElement content = new XElement("TileAnimator", new XAttribute("startTimeMs", mesh.StartTimeMs), new XAttribute("durationMs", mesh.DurationMs), new XAttribute("fullTimeMs", mesh.FullAnimationDurationMs));
                //    xElement.Add(content);
                }
            }
            return gameObjects;
        }

        private void AssignUnityProperties(TmxHasProperties tmxHasProperties, XElement xml, PrefabContext context)
        {
            TmxProperties propertiesWithTypeDefaults = TmxHelper.GetPropertiesWithTypeDefaults(tmxHasProperties, tmxMap.ObjectTypes);
            string propertyValueAsString = propertiesWithTypeDefaults.GetPropertyValueAsString("unity:scale", "");
            if (!string.IsNullOrEmpty(propertyValueAsString))
            {
                float result = 1f;
                if (context != 0)
                {
                    Logger.WriteWarning("unity:scale only applies to map properties\n{0}", xml.ToString());
                }
                else if (!float.TryParse(propertyValueAsString, out result))
                {
                    Logger.WriteError("unity:scale property value '{0}' could not be converted to a float", propertyValueAsString);
                }
                else
                {
                    xml.SetAttributeValue("scale", propertyValueAsString);
                }
            }
            string propertyValueAsString2 = propertiesWithTypeDefaults.GetPropertyValueAsString("unity:resource", "");
            if (!string.IsNullOrEmpty(propertyValueAsString2))
            {
                bool result2 = false;
                if (context != 0)
                {
                    Logger.WriteWarning("unity:resource only applies to map properties\n{0}", xml.ToString());
                }
                else if (!bool.TryParse(propertyValueAsString2, out result2))
                {
                    Logger.WriteError("unity:resource property value '{0}' could not be converted to a boolean", propertyValueAsString2);
                }
                else
                {
                    xml.SetAttributeValue("resource", propertyValueAsString2);
                }
            }
            string unityResourcePath = propertiesWithTypeDefaults.GetPropertyValueAsString("unity:resourcePath", "");
            if (!string.IsNullOrEmpty(unityResourcePath))
            {
                if (context != 0)
                {
                    Logger.WriteWarning("unity:resourcePath only applies to map properties\n{0}", xml.ToString());
                }
                else if (Path.GetInvalidPathChars().Any((char c) => unityResourcePath.Contains(c)))
                {
                    Logger.WriteError("unity:resourcePath has invalid path characters: {0}", unityResourcePath);
                }
                else
                {
                    xml.SetAttributeValue("resourcePath", unityResourcePath);
                }
            }
            string propertyValueAsString3 = propertiesWithTypeDefaults.GetPropertyValueAsString("unity:isTrigger", "");
            if (!string.IsNullOrEmpty(propertyValueAsString3))
            {
                bool result3 = false;
                if (!bool.TryParse(propertyValueAsString3, out result3))
                {
                    Logger.WriteError("unity:isTrigger property value '{0}' cound not be converted to a boolean", propertyValueAsString3);
                }
                else
                {
                    xml.SetAttributeValue("isTrigger", propertyValueAsString3);
                }
            }
            string propertyValueAsString4 = propertiesWithTypeDefaults.GetPropertyValueAsString("unity:layer", "");
            if (!string.IsNullOrEmpty(propertyValueAsString4))
            {
                xml.SetAttributeValue("layer", propertyValueAsString4);
            }
            string propertyValueAsString5 = propertiesWithTypeDefaults.GetPropertyValueAsString("unity:tag", "");
            if (!string.IsNullOrEmpty(propertyValueAsString5))
            {
                xml.SetAttributeValue("tag", propertyValueAsString5);
            }
            List<string> knownProperties = new List<string>();
            knownProperties.Add("unity:layer");
            knownProperties.Add("unity:tag");
            knownProperties.Add("unity:sortingLayerName");
            knownProperties.Add("unity:sortingOrder");
            knownProperties.Add("unity:scale");
            knownProperties.Add("unity:isTrigger");
            knownProperties.Add("unity:convex");
            knownProperties.Add("unity:ignore");
            knownProperties.Add("unity:resource");
            knownProperties.Add("unity:resourcePath");
            foreach (string item in from p in propertiesWithTypeDefaults.PropertyMap
                                    where p.Key.StartsWith("unity:")
                                    where !knownProperties.Contains(p.Key)
                                    select p.Key)
            {
                Logger.WriteWarning("Unknown unity property '{0}' in GameObject '{1}'", item, tmxHasProperties.ToString());
            }
        }

        private void AssignTiledProperties(TmxHasProperties tmxHasProperties, XElement xml)
        {
            TmxProperties propertiesWithTypeDefaults = TmxHelper.GetPropertiesWithTypeDefaults(tmxHasProperties, tmxMap.ObjectTypes);
            List<XElement> list = new List<XElement>();
            foreach (KeyValuePair<string, TmxProperty> item2 in propertiesWithTypeDefaults.PropertyMap)
            {
                if (!item2.Key.StartsWith("unity:") && (from p in xml.Elements("Property")
                                                        where p.Attribute("name") != null
                                                        where p.Attribute("name").Value == item2.Key
                                                        select p).Count() <= 0)
                {
                    XElement item = new XElement("Property", new XAttribute("name", item2.Key), new XAttribute("value", item2.Value.Value));
                    list.Add(item);
                }
            }
            xml.Add(list);
        }

        private UnityEngine.Collider2D CreateBoxColliderElement(UnityEngine.GameObject obj,TmxObjectRectangle tmxRectangle)
        {
            var box = obj.AddComponent<UnityEngine.BoxCollider2D>();
            box.size = new UnityEngine.Vector2(tmxRectangle.Size.Width * Settings.Scale, tmxRectangle.Size.Height * Settings.Scale);

            return box;
            //    return new XElement("BoxCollider2D", new XAttribute("width", tmxRectangle.Size.Width * Settings.Scale), new XAttribute("height", tmxRectangle.Size.Height * Settings.Scale));
        }

        private UnityEngine.Collider2D CreateCircleColliderElement(UnityEngine.GameObject obj, TmxObjectEllipse tmxEllipse, string objGroupName)
        {
            var ad = obj.AddComponent<UnityEngine.CircleCollider2D>();
            if (tmxMap.Orientation == TmxMap.MapOrientation.Isometric)
            {
                Logger.WriteError("Collision ellipse in Object Layer '{0}' is not supported in Isometric maps: {1}", objGroupName, tmxEllipse);
                return null;
            }
            if (!tmxEllipse.IsCircle())
            {
                Logger.WriteError("Collision ellipse in Object Layer '{0}' is not a circle: {1}", objGroupName, tmxEllipse);
                return null;
            }
            ad.radius = tmxEllipse.Radius * Settings.Scale;

            return ad;//new XElement("CircleCollider2D", new XAttribute("radius", tmxEllipse.Radius * Settings.Scale));
        }

        private UnityEngine.Collider2D CreatePolygonColliderElement(UnityEngine.GameObject obj,TmxObjectPolygon tmxPolygon)
        {
            var pw = obj.AddComponent<UnityEngine.PolygonCollider2D>();
            pw.pathCount = 1;

            
            IEnumerable<PointF> source = from pt in TmxMath.GetPointsInMapSpace(tmxMap, tmxPolygon)
                                         select PointFToUnityVector(pt);
            List<UnityEngine.Vector2> v2 = new List<UnityEngine.Vector2>();
            foreach (var v in source)
            {
                v2.Add(new UnityEngine.Vector2(v.X, v.Y));

            }
            pw.SetPath(0, v2.ToArray());
            return pw;
            //  return new XElement("PolygonCollider2D", new XElement("Path", string.Join(" ", from pt in source
            //                               select $"{pt.X},{pt.Y}")));
        }

        private UnityEngine.Collider2D CreateEdgeColliderElement(UnityEngine.GameObject obj,TmxObjectPolyline tmxPolyline)
        {
            var ad = obj.AddComponent<UnityEngine.EdgeCollider2D>();


            IEnumerable<PointF> source = from pt in TmxMath.GetPointsInMapSpace(tmxMap, tmxPolyline)
                                         select PointFToUnityVector(pt);
            List<UnityEngine.Vector2> V2 = new List<UnityEngine.Vector2>();
            foreach(var v in source)
            {
                V2.Add(new UnityEngine.Vector2(v.X, v.Y));
            }
            ad.points = V2.ToArray();
            return ad;
        }

        private void AddTileObjectElements(TmxObjectTile tmxObjectTile, UnityEngine.GameObject xmlTileObjectRoot, Tiled2Unity.TileObject objComponent)
        {
            SizeF tileObjectScale = tmxObjectTile.GetTileObjectScale();
            float num = tmxObjectTile.FlippedHorizontal ? (-1f) : 1f;
            float num2 = tmxObjectTile.FlippedVertical ? (-1f) : 1f;
            float num3 = (float)tmxObjectTile.Tile.TileSize.Width;
            float num4 = (float)tmxObjectTile.Tile.TileSize.Height;
            float num5 = num3 * 0.5f;
            float num6 = num4 * 0.5f;
            xmlTileObjectRoot.transform.localScale = new UnityEngine.Vector3(tileObjectScale.Width, tileObjectScale.Height, xmlTileObjectRoot.transform.localScale.z);

            //xmlTileObjectRoot.SetAttributeValue("scaleX", tileObjectScale.Width);
            //xmlTileObjectRoot.SetAttributeValue("scaleY", tileObjectScale.Height);
            //      AssignTiledProperties(tmxObjectTile.Tile, xmlTileObjectRoot);
            //objComponent.Name = "TileObjectComponent";
            //TmxObject2 w;
   var a=         xmlTileObjectRoot.AddComponent<TileObject>();
            a.TmxFlippingHorizontal = tmxObjectTile.FlippedHorizontal;
            a.TmxFlippingVertical = tmxObjectTile.FlippedVertical;
            a.TileWidth = (float)tmxObjectTile.Tile.TileSize.Width * tileObjectScale.Width * Settings.Scale;
            a.TileHeight = (float)tmxObjectTile.Tile.TileSize.Height * tileObjectScale.Height * Settings.Scale;

            //objComponent.SetAttributeValue("tmx-tile-flip-horizontal", tmxObjectTile.FlippedHorizontal);
            //objComponent.SetAttributeValue("tmx-tile-flip-vertical", tmxObjectTile.FlippedVertical);
            //objComponent.SetAttributeValue("width", (float)tmxObjectTile.Tile.TileSize.Width * tileObjectScale.Width * Settings.Scale);
            //objComponent.SetAttributeValue("height", (float)tmxObjectTile.Tile.TileSize.Height * tileObjectScale.Height * Settings.Scale);
            //    xmlTileObjectRoot.Add(objComponent);

            var xElement = new UnityEngine.GameObject("TileObject");// new XElement("GameObject");
         
            //   xElement.SetAttributeValue("name", "TileObject");
            if (tmxMap.Orientation == TmxMap.MapOrientation.Isometric)
            {
                xElement.transform.position = new UnityEngine.Vector3(0, num6 * Settings.Scale, xElement.transform.position.z);
                //xElement.SetAttributeValue("x", 0);
                //xElement.SetAttributeValue("y", num6 * Settings.Scale);
            }
            else

            {
                xElement.transform.position = new UnityEngine.Vector3(num5 * Settings.Scale, num6 * Settings.Scale, xElement.transform.position.z);

                //xElement.SetAttributeValue("x", num5 * Settings.Scale);
                //xElement.SetAttributeValue("y", num6 * Settings.Scale);
            }
            xElement.transform.localScale = new UnityEngine.Vector3(num, num2, xElement.transform.position.z);
            //xElement.SetAttributeValue("scaleX", num);
            //xElement.SetAttributeValue("scaleY", num2);
            TmxMap.MapOrientation orientation = tmxMap.Orientation;
            tmxMap.Orientation = TmxMap.MapOrientation.Orthogonal;
            if (tmxObjectTile.ParentObjectGroup.Ignore != TmxLayerNode.IgnoreSettings.Collision)
            {
                foreach (TmxObject @object in tmxObjectTile.Tile.ObjectGroup.Objects)
                {
                    UnityEngine.Collider2D xElement2 = null;
                    if (@object.GetType() == typeof(TmxObjectRectangle))
                    {
                        xElement2 = CreateBoxColliderElement(xElement,@object as TmxObjectRectangle);
                    }
                    else if (@object.GetType() == typeof(TmxObjectEllipse))
                    {
                        xElement2 = CreateCircleColliderElement(xElement,@object as TmxObjectEllipse, tmxObjectTile.Tile.ObjectGroup.Name);
                    }
                    else if (@object.GetType() == typeof(TmxObjectPolygon))
                    {
                        xElement2 = CreatePolygonColliderElement(xElement,@object as TmxObjectPolygon);
                    }
                    else if (@object.GetType() == typeof(TmxObjectPolyline))
                    {
                        xElement2 = CreateEdgeColliderElement(xElement,@object as TmxObjectPolyline);
                    }
                    if (xElement2 != null)
                    {
                        float num7 = (0f - num5 + @object.Position.X) * Settings.Scale;
                        float num8 = (num6 - @object.Position.Y) * Settings.Scale;
                     
                        //xElement2.SetAttributeValue("offsetX", num7);
                        //xElement2.SetAttributeValue("offsetY", num8);
                        TmxObjectType valueOrDefault = tmxObjectTile.ParentObjectGroup.ParentMap.ObjectTypes.GetValueOrDefault(@object.Type);
                        string text = string.IsNullOrEmpty(valueOrDefault.Name) ? "Default" : valueOrDefault.Name;
                        string value = "Collision_" + text;
                        var obj = new UnityEngine.GameObject(value);
                        obj.layer = UnityEngine.LayerMask.NameToLayer(text);

                        obj.transform.parent = xElement.transform;
                        if (@object.GetType() == typeof(TmxObjectRectangle))
                        {
                            xElement2 = CreateBoxColliderElement(obj, @object as TmxObjectRectangle);
                        }
                        else if (@object.GetType() == typeof(TmxObjectEllipse))
                        {
                            xElement2 = CreateCircleColliderElement(obj, @object as TmxObjectEllipse, tmxObjectTile.Tile.ObjectGroup.Name);
                        }
                        else if (@object.GetType() == typeof(TmxObjectPolygon))
                        {
                            xElement2 = CreatePolygonColliderElement(obj, @object as TmxObjectPolygon);
                        }
                        else if (@object.GetType() == typeof(TmxObjectPolyline))
                        {
                            xElement2 = CreateEdgeColliderElement(obj, @object as TmxObjectPolyline);
                        }
                        xElement2.offset = new UnityEngine.Vector2(num7, num8);
                        //  XElement xElement3 = new XElement("GameObject");
                        //  xElement3.SetAttributeValue("name", value);
                        //  xElement3.SetAttributeValue("layer", text);
                        //  xElement3.Add(xElement2);
                        // xElement.Add(xElement3);
                    }
                }
            }
            tmxMap.Orientation = orientation;
            if (tmxObjectTile.ParentObjectGroup.Ignore != TmxLayerNode.IgnoreSettings.Visual)
            {
                foreach (TmxMesh mesh in tmxObjectTile.Tile.Meshes)
                {
                    var xElement4 = new UnityEngine.GameObject(mesh.ObjectName);
               var ad=     xElement4.AddComponent<UnityEngine.MeshFilter>();
                  var b=  xElement4.AddComponent<UnityEngine.MeshRenderer>();
                    ad.mesh =meshes.Find(x=>x.name== mesh.UniqueMeshName);
                    b.material = materials.Find(x => x.name == mtm[mesh.UniqueMeshName]);
                    //XElement xElement4 = new XElement("GameObject");

                    b.sortingLayerName = tmxObjectTile.GetSortingLayerName();
                    b.sortingOrder = tmxObjectTile.GetSortingOrder();
                    var opacity = tmxObjectTile.ParentObjectGroup.GetRecursiveOpacity();
                    if (opacity == 1.0f)
                        return;

#if UNITY_5_6_OR_NEWER
                    // Add a component that will control our instanced shader properties
                    Tiled2Unity.GPUInstancing instancing = xElement4.GetOrAddComponent<Tiled2Unity.GPUInstancing>();
                    instancing.Opacity = opacity;
#endif
                    xElement4.transform.position = new UnityEngine.Vector3((0f - num5) * Settings.Scale, num6 * Settings.Scale, 0);

                    //xElement4.SetAttributeValue("name", mesh.ObjectName);
                    //xElement4.SetAttributeValue("mesh", mesh.UniqueMeshName);
                    //xElement4.SetAttributeValue("sortingLayerName", tmxObjectTile.GetSortingLayerName());
                    //xElement4.SetAttributeValue("sortingOrder", tmxObjectTile.GetSortingOrder());
                    //xElement4.SetAttributeValue("opacity", tmxObjectTile.ParentObjectGroup.GetRecursiveOpacity());
                    //xElement4.SetAttributeValue("x", (0f - num5) * Settings.Scale);
                    //xElement4.SetAttributeValue("y", num6 * Settings.Scale);
                    if (mesh.FullAnimationDurationMs > 0)
                    {
                        var ta = xElement4.AddComponent<TileAnimator>();
                        ta.StartTime = mesh.StartTimeMs * 0.001f;
                        ta.Duration = mesh.DurationMs* 0.001f;
                        ta.TotalAnimationTime = mesh.FullAnimationDurationMs * 0.001f;
                        
                    //    XElement content = new XElement("TileAnimator", new XAttribute("startTimeMs", mesh.StartTimeMs), new XAttribute("durationMs", mesh.DurationMs), new XAttribute("fullTimeMs", mesh.FullAnimationDurationMs));
                      //  xElement4.Add(content);
                    }
                    xElement4.transform.SetParent(xElement.transform);
                   // xElement.Add(xElement4);
                }
            }
            xElement.transform.SetParent(xmlTileObjectRoot.transform);
         //   xmlTileObjectRoot.Add(xElement);
        }
    }
}
