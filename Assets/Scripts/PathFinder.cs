using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MapGen;

public class Node
{
    public Node cameFrom = null; //parent node
    public double priority = 0; // F value
    public double costSoFar = 0; // G Value
    public Tile tile;

    public Node(Tile _tile, double _priority, Node _cameFrom, double _costSoFar)
    {
        cameFrom = _cameFrom;
        priority = _priority; 
        costSoFar = _costSoFar;
        tile = _tile;
    }

    public override string ToString() {
        return tile.ToString();
    }
}

public class PathFinder
{
    List<Node> TODOList = new List<Node>();
    List<Node> DoneList = new List<Node>();
    Tile goalTile = null;

    public PathFinder(){
        
    }

    public void clearASTARLists() {
        TODOList.Clear();
        DoneList.Clear();
    }

    // TODO: Find the path based on A-Star Algorithm
    public Queue<Tile> FindPathAStar(Tile start, Tile goal)
    {
        // Starting node (The tile the player starts on)
        Node startNode = new Node(start, HeuristicsDistance(start, goal), null, 0);
        Node currentNode = startNode;

        TODOList.Add(startNode);

        while (TODOList.Count > 0) {
            // Sort the TODO list so we can get the lowest F value
            Node temp = TODOList[0];
            for(int i=0; i<TODOList.Count; i++) {
                if (TODOList[i].priority < temp.priority)
                    temp = TODOList[i];
            }

            currentNode = temp;

            // Found the goal
            // DONT REMOVE CURLY BRACKETS FROM IF STATEMENT
            // For some odd reason, it really wants those brackets or it goes boom
            if (currentNode.tile == goal) {
                return RetracePath(currentNode);
            }

            // Remove the current node from the todo list and add it to done list.
            TODOList.Remove(currentNode);
            DoneList.Add(currentNode);

            // List of adjacent tiles to current tile
            List<Tile> adjacentList = new List<Tile>(currentNode.tile.Adjacents);

            // Go through the list of adjacents
            foreach (Tile currentTile in adjacentList) {
                // Create the node for the tile
                double g = HeuristicsDistance(currentNode.tile, currentTile);
                double h = HeuristicsDistance(currentTile, goal);
                double f = g + h;
                Node newChild = new Node(currentTile, f, currentNode, g);

                // Child is on the done list
                if (!DoneList.Contains(newChild)) {
                    if (!TODOList.Contains(newChild))
                        TODOList.Add(newChild);
                    else {
                        Node TODOAdjacent = TODOList[TODOList.IndexOf(newChild)];
                        if(newChild.costSoFar < TODOAdjacent.costSoFar) {
                            TODOAdjacent.costSoFar = newChild.costSoFar;
                            TODOAdjacent.cameFrom = newChild.cameFrom;
                        }
                    }
                }
            }
            // No path was found
            return new Queue<Tile>();
        }
        // No path was found
        return new Queue<Tile>();
    }

    // TODO: Find the path based on A-Star Algorithm
    // In this case avoid a path passing near an enemy tile
    public Queue<Tile> FindPathAStarEvadeEnemy(Tile start, Tile goal)
    {
        // Starting node (The tile the player starts on)
        Node startNode = new Node(start, HeuristicsDistance(start, goal), null, 0);
        Node currentNode = startNode;

        TODOList.Add(startNode);

        while (TODOList.Count > 0) {
            // Sort the TODO list so we can get the lowest F value
            Node temp = TODOList[0];
            for (int i = 0; i < TODOList.Count; i++) {
                if (TODOList[i].priority < temp.priority)
                    temp = TODOList[i];
            }

            currentNode = temp;

            // Found the goal
            // DONT REMOVE CURLY BRACKETS FROM IF STATEMENT
            // For some odd reason, it really wants those brackets or it goes boom
            if (currentNode.tile == goal) {
                clearASTARLists();
                return RetracePath(currentNode);
            }

            // Remove the current node from the todo list and add it to done list.
            TODOList.Remove(currentNode);
            DoneList.Add(currentNode);

            // List of adjacent tiles to current tile
            List<Tile> adjacentList = new List<Tile>(currentNode.tile.Adjacents);

            // Go through the list of adjacents
            foreach (Tile currentTile in adjacentList) {
                // Create the node for the tile
                double g = HeuristicsDistance(currentNode.tile, currentTile);
                double h = HeuristicsDistance(currentTile, goal);
                double f = g + h;
                Node newChild = new Node(currentTile, f, currentNode, g);

                // Child is on the done list
                if (!DoneList.Contains(newChild)) {
                    if (!TODOList.Contains(newChild))
                        TODOList.Add(newChild);
                    else {
                        Node TODOAdjacent = TODOList[TODOList.IndexOf(newChild)];
                        if (newChild.costSoFar < TODOAdjacent.costSoFar) {
                            TODOAdjacent.costSoFar = newChild.costSoFar;
                            TODOAdjacent.cameFrom = newChild.cameFrom;
                        }
                    }
                }
            }
            // No path was found
            return new Queue<Tile>();
        }
        // No path was found
        return new Queue<Tile>();
    }

    // Manhattan Distance with horizontal/vertical cost of 10
    double HeuristicsDistance(Tile currentTile, Tile goalTile)
    {
        int xdist = Math.Abs(goalTile.indexX - currentTile.indexX);
        int ydist = Math.Abs(goalTile.indexY - currentTile.indexY);
        // Assuming cost to move horizontally and vertically is 10
        //return manhattan distance
        return (xdist * 10 + ydist * 10);
    }

    // Retrace path from a given Node back to the start Node
    Queue<Tile> RetracePath(Node node)
    {
        List<Tile> tileList = new List<Tile>();
        Node nodeIterator = node;
        while (nodeIterator.cameFrom != null)
        {
            tileList.Insert(0, nodeIterator.tile);
            nodeIterator = nodeIterator.cameFrom;
        }
        return new Queue<Tile>(tileList);
    }

    // Generate a Random Path. Used for enemies
    public Queue<Tile> RandomPath(Tile start, int stepNumber)
    {
        List<Tile> tileList = new List<Tile>();
        Tile currentTile = start;
        for (int i = 0; i < stepNumber; i++)
        {
            Tile nextTile;
            //find random adjacent tile different from last one if there's more than one choice
            if (currentTile.Adjacents.Count < 0)
            {
                break;
            }
            else if (currentTile.Adjacents.Count == 1)
            {
                nextTile = currentTile.Adjacents[0];
            }
            else
            {
                nextTile = null;
                List<Tile> adjacentList = new List<Tile>(currentTile.Adjacents);
                ShuffleTiles<Tile>(adjacentList);
                if (tileList.Count <= 0) nextTile = adjacentList[0];
                else
                {
                    foreach (Tile tile in adjacentList)
                    {
                        if (tile != tileList[tileList.Count - 1])
                        {
                            nextTile = tile;
                            break;
                        }
                    }
                }
            }
            tileList.Add(currentTile);
            currentTile = nextTile;
        }
        return new Queue<Tile>(tileList);
    }

    private void ShuffleTiles<T>(List<T> list)
    {
        // Knuth shuffle algorithm :: 
        // courtesy of Wikipedia :) -> https://forum.unity.com/threads/randomize-array-in-c.86871/
        for (int t = 0; t < list.Count; t++)
        {
            T tmp = list[t];
            int r = UnityEngine.Random.Range(t, list.Count);
            list[t] = list[r];
            list[r] = tmp;
        }
    }
}
