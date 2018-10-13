using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class a12 : MonoBehaviour {

    public string pat = @"F:\Program Files\Tiled2Unity\Examples\hex-flat.tmx";
    // Use this for initialization
    void Start () {
       var map= Tiled2Unity.TmxMap.LoadFromFile(pat);
        Tiled2Unity.TiledMapExporter w = new Tiled2Unity.TiledMapExporter(map);
        w.Export("");
       // GetComponent<MeshRenderer>().material = w.Materials[0];
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
