using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapGen;

public class Tile : MonoBehaviour
{
    public MapTile mapTile;
    public int indexX;
    public int indexY;
    public bool isPassable;
    public bool isStart { get; private set; }
    public bool isGoal { get; private set; }
    public List<Tile> Adjacents { get; private set; }

    private void Awake()
    {
        Adjacents = new List<Tile>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void FillFromMapTile(MapTile _mapTile)
    {
        mapTile = _mapTile;
        indexX = mapTile.X;
        indexY = mapTile.Y;
        isPassable = mapTile.Walkable;
        isStart = mapTile.IsStart;
        isGoal = mapTile.IsGoal;
    }

    public void AddAdjacentTile(Tile tile)
    {
        if (tile.isPassable) Adjacents.Add(tile);
    }

    public override string ToString()
    {
        return "( " + indexX + ", " + indexY + " )";
    }
}
