using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapGen;

public enum MapState {GENERATED, DESTROYED}

public class GenerateMap : MonoBehaviour
{
    public int width = 20;
    public int height = 20;
    public bool perlinGenerator = false;
    public float perlinConstraint = 2.0f;
    public float primeConstraint = 0.5f;

    public GameObject passableTilePrefab;
    public GameObject nonPassableTilePrefab;
    public Material startTileMaterial;
    public Material goalTileMaterial;
    public MapState state = MapState.DESTROYED;

    MapTile[,] mapTileList;
    Tile[,] tileList;
    public Tile start;
    public Tile goal;

    GameObject playerGameObject;

    // Start is called before the first frame update
    void Start()
    {
        playerGameObject = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        switch(state)
        {
            case MapState.DESTROYED:
                transform.localScale = new Vector3(1, 1, 1);
                GenerateNewMap();
                transform.localScale = new Vector3(20f / width, 1, 20f / height);
                state = MapState.GENERATED;
                break;
            case MapState.GENERATED:
                if(Input.GetKey(KeyCode.Space))
                {
                    foreach (Transform child in transform)
                    {
                        GameObject.Destroy(child.gameObject);
                    }
                    state = MapState.DESTROYED;
                }
                break;
            default:
                break;
        }
    }

    public void GenerateNewMap()
    {
        mapTileList = null;
        mapTileList = generateTilesList();
        tileList = null;
        tileList = new Tile[width, height];
        foreach (MapTile mapTile in mapTileList)
        {
            Tile t = CreateTile(mapTile);
            tileList[mapTile.X, mapTile.Y] = t;
        }
        foreach (Tile tile in tileList)
        {
            FillAdjacentsForaTile(tile);
        }
        CreateWalls();
        ResetEnemies();
    }

    MapTile[,] generateTilesList()
    {
        MapTile[,] tiles;
        if (perlinGenerator)
        {
            PerlinGenerator perlinGen = new PerlinGenerator();
            tiles = perlinGen.MapGen(width, height, perlinConstraint);
        }
        else
        {
            PrimGenerator primGen = new PrimGenerator();
            tiles = primGen.MapGen(width, height, primeConstraint);
        }
        return tiles;
    }

    private Tile CreateTile(MapTile mapTile)
    {
        GameObject tileGO;
        if (mapTile.Walkable) tileGO = Instantiate(passableTilePrefab, transform);
        else tileGO = Instantiate(nonPassableTilePrefab, transform);

        tileGO.name += tileGO.transform.GetSiblingIndex().ToString();
        tileGO.transform.position = new Vector3(-width / 2, 0, -height / 2) + new Vector3(mapTile.X, 0, mapTile.Y);
        Tile tile = tileGO.GetComponent<Tile>();
        tile.FillFromMapTile(mapTile);
        if (tile.isStart)
        {
            tileGO.GetComponent<MeshRenderer>().material = startTileMaterial;
            start = tile;
            if (playerGameObject != null)
            {
                playerGameObject.transform.position = tileGO.transform.position;
                playerGameObject.GetComponent<Player>().Reset(tileGO.GetComponent<Tile>());
            }
        }
        else if (tile.isGoal)
        {
            tileGO.GetComponent<MeshRenderer>().material = goalTileMaterial;
            goal = tile;
        }
        return tile;
    }

    void FillAdjacentsForaTile(Tile tile)
    {
        //left
        if (tile.indexX - 1 >= 0) tile.AddAdjacentTile(tileList[tile.indexX - 1, tile.indexY]);
        //right
        if (tile.indexX + 1 < width) tile.AddAdjacentTile(tileList[tile.indexX + 1, tile.indexY]);
        //up
        if (tile.indexY - 1 >= 0) tile.AddAdjacentTile(tileList[tile.indexX, tile.indexY - 1]);
        //down
        if (tile.indexY + 1 < height) tile.AddAdjacentTile(tileList[tile.indexX, tile.indexY + 1]);

        // diagonal tiles
        // NOTE: Diagonal motion across a wall tile is not allowed
        if (tile.indexX - 1 >= 0 && tile.indexY - 1 >= 0)
        {
            if(tileList[tile.indexX - 1, tile.indexY].isPassable && tileList[tile.indexX, tile.indexY - 1].isPassable)
            {
                tile.AddAdjacentTile(tileList[tile.indexX - 1, tile.indexY - 1]);
            }   
        }
        if (tile.indexX - 1 >= 0 && tile.indexY + 1 < height)
        {
            if (tileList[tile.indexX - 1, tile.indexY].isPassable && tileList[tile.indexX, tile.indexY + 1].isPassable)
            {
                tile.AddAdjacentTile(tileList[tile.indexX - 1, tile.indexY + 1]);
            }
        }
        if (tile.indexX + 1 < width && tile.indexY - 1 >= 0)
        {
            if (tileList[tile.indexX, tile.indexY - 1].isPassable && tileList[tile.indexX + 1, tile.indexY].isPassable)
            {
                tile.AddAdjacentTile(tileList[tile.indexX + 1, tile.indexY - 1]);
            }
        }
        if (tile.indexX + 1 < width && tile.indexY + 1 < height)
        {
            if (tileList[tile.indexX , tile.indexY + 1].isPassable && tileList[tile.indexX + 1, tile.indexY].isPassable)
            {
                tile.AddAdjacentTile(tileList[tile.indexX + 1, tile.indexY + 1]);
            }
        }
    }

    //This Function creates Walls around the map
    void CreateWalls()
    {
        for (int i = 0; i < width + 2; i++)
        {
            //upper wall
            GameObject upperWallGO = Instantiate(nonPassableTilePrefab, transform);
            upperWallGO.transform.position = new Vector3(-width / 2, 0, -height / 2) + new Vector3(i - 1, 0, -1);
            upperWallGO.GetComponent<Tile>().isPassable = false;

            //lower wall
            GameObject lowerWallGO = Instantiate(nonPassableTilePrefab, transform);
            lowerWallGO.transform.position = new Vector3(-width / 2, 0, -height / 2) + new Vector3(i - 1, 0, height);
            lowerWallGO.GetComponent<Tile>().isPassable = false;
        }

        for (int j = 0; j < height + 2; j++)
        {
            //left wall
            GameObject leftWallGO = Instantiate(nonPassableTilePrefab, transform);
            leftWallGO.transform.position = new Vector3(-width / 2, 0, -height / 2) + new Vector3(-1, 0, j - 1);
            leftWallGO.GetComponent<Tile>().isPassable = false;

            //rightwall
            GameObject rightWallGO = Instantiate(nonPassableTilePrefab, transform);
            rightWallGO.transform.position = new Vector3(-width / 2, 0, -height / 2) + new Vector3(width, 0, j - 1);
            rightWallGO.GetComponent<Tile>().isPassable = false;

        }
    }

    private void ResetEnemies()
    {
        Enemy[] enemyList = GameObject.FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemyList)
        {
            enemy.Reset();
        }
    }
}
