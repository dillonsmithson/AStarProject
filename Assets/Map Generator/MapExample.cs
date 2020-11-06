using UnityEngine;
using System.Collections;
// includes the MapGen namespace methods
using MapGen;

public class MapExample : MonoBehaviour
{ 
	void Start ()
    {
        PrimGenerator primGen = new PrimGenerator();

        // generate a map of size 20x20 with no extra walls removed
        MapTile[,] tiles1 = primGen.MapGen(20, 20, 0.0f);

        // generate a map of size 30x30 with half of the walls removed after generation
        MapTile[,] tiles2 = primGen.MapGen(30, 30, 0.5f);

        PerlinGenerator perlinGen = new PerlinGenerator();

        // generates a map of size 20x20 with a large constraint (generates a tightly-packed map)
        MapTile[,] tiles3 = perlinGen.MapGen(20, 20, 5.0f);

        // generates a map of size 20x20 with a medium constraint (generates a more open map)
        MapTile[,] tiles4 = perlinGen.MapGen(20, 20, 10.0f);

        // generates a map of size 20x20 with a small constraint (generates a very open map)
        MapTile[,] tiles5 = perlinGen.MapGen(20, 20, 20.0f);
    }
}
