using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiled2Unity
{
  public  class TmxIaMesh
    {
        public static Dictionary<string, TmxIaMesh> meshs = new Dictionary<string, TmxIaMesh>();
        public static Dictionary<string, UnityEngine.Material> ms = new Dictionary<string, UnityEngine.Material>();




        public List<UnityEngine.Vector3> Vertices = new List<UnityEngine.Vector3>();
        public List<UnityEngine.Vector2> uv = new List<UnityEngine.Vector2>();
        public UnityEngine.Vector3 normal = new UnityEngine.Vector3();
        public List<int> Faces = new List<int>();


        public TmxIaMesh()
        {

            normal = new UnityEngine.Vector3(0, 0, -1);
        }
        public UnityEngine.Mesh ab;
        public UnityEngine.Mesh ToMesh()
        {
            return ab;
            var mesh = new UnityEngine.Mesh();
           
         //   mesh.uv=uv.ToArray();
          //  mesh.triangles = Faces.ToArray();
          for(int i=0;i<Vertices.Count;i++)
            {
                var v = Vertices[i];
                v.x -= 30;
                Vertices[i] = v;
                //  Vertices[i]=new UnityEngine.Vector3();
            }
            mesh.vertices = (Vertices).ToArray();
            UnityEngine.Debug.Log("count" + uv.Count);
            var wr = Faces.ToArray();
            Array.Resize(ref wr, Vertices.Count);
            mesh.SetIndices(wr,UnityEngine.MeshTopology.Triangles,0);

         
            var tw = uv.ToArray();
            Array.Resize(ref tw, Vertices.Count);
        
            
            // uv.ToArray();
        //var sz=    new UnityEngine.Vector3[] { normal };
        //    Array.Resize(ref sz, Vertices.Count);
        //    mesh.normals = sz;
    
            mesh.SetUVs(0, tw.ToList());
            //   mesh.normals = new UnityEngine.Vector3[] { normal };
            return mesh;
        }

        public void AddVert(UnityEngine.Vector3 vector)
        {
            Vertices.Add(vector);
        }
        public void AddUv(UnityEngine.Vector2 vector)
        {
            uv.Add(vector);
        }
        public void AddTriganles(int a,int b,int c,int d)
        {
            Faces.Add(a);
            Faces.Add(b);
            Faces.Add(c);
            Faces.Add(d);
        }
    }
}
