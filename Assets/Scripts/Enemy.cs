using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// FSM States for the enemy
public enum EnemyState { STATIC, CHASE, REST, MOVING, DEFAULT };

public enum EnemyBehavior {EnemyBehavior1, EnemyBehavior2, EnemyBehavior3 };

public class Enemy : MonoBehaviour
{
    //pathfinding
    protected PathFinder pathFinder;
    public GenerateMap mapGenerator;
    protected Queue<Tile> path1;
    protected Queue<Tile> path2;
    protected Queue<Tile> path3;
    protected GameObject playerGameObject;

    public Tile currentTile;
    protected Tile targetTile;
    public Vector3 velocity;

    //properties
    int playeCloseCounter = 0;
    int maxCount = 5;
    public float speed = 0.05f;
    public float visionDistance = 5;
    public int maxCounter = 5;
    protected int playerCloseCounter;

    protected EnemyState state = EnemyState.DEFAULT;
    protected Material material;

    public EnemyBehavior behavior = EnemyBehavior.EnemyBehavior1;
    public Player player;

    // Start is called before the first frame update
    void Start()
    {
        path1 = new Queue<Tile>();
        path2 = new Queue<Tile>();
        path3 = new Queue<Tile>();
        pathFinder = new PathFinder();
        playerGameObject = GameObject.FindWithTag("Player");
        playerCloseCounter = maxCounter;
        material = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (mapGenerator.state == MapState.DESTROYED) return;

        // Stop Moving the enemy if the player has reached the goal
        if (playerGameObject.GetComponent<Player>().IsGoalReached() || playerGameObject.GetComponent<Player>().IsPlayerDead())
        {
            //Debug.Log("Enemy stopped since the player has reached the goal or the player is dead");
            return;
        }

        switch(behavior)
        {
            case EnemyBehavior.EnemyBehavior1:
                HandleEnemyBehavior1();
                break;
            case EnemyBehavior.EnemyBehavior2:
                HandleEnemyBehavior2();
                break;
            case EnemyBehavior.EnemyBehavior3:
                HandleEnemyBehavior3();
                break;
            default:
                break;
        }

    }

    public void Reset()
    {
        Debug.Log("enemy reset");
        path1.Clear();
        path2.Clear();
        path3.Clear();
        state = EnemyState.DEFAULT;
        currentTile = FindWalkableTile();
        transform.position = currentTile.transform.position;
    }

    Tile FindWalkableTile()
    {
        Tile newTarget = null;
        int randomIndex = 0;
        while (newTarget == null || !newTarget.mapTile.Walkable)
        {
            randomIndex = (int)(Random.value * mapGenerator.width * mapGenerator.height - 1);
            newTarget = GameObject.Find("MapGenerator").transform.GetChild(randomIndex).GetComponent<Tile>();
        }
        return newTarget;
    }

    // Dumb Enemy: Keeps Walking in Random direction, Will not chase player
    private void HandleEnemyBehavior1()
    {
        switch (state)
        {
            case EnemyState.DEFAULT: // generate random path 
                material.color = Color.white;
                if (path1.Count <= 0) path1 = pathFinder.RandomPath(currentTile, 20);

                if (path1.Count > 0)
                {
                    targetTile = path1.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;

            case EnemyState.MOVING:
                //move
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed);
                
                //if target reached
                if (Vector3.Distance(transform.position, targetTile.gameObject.transform.position) <= speed)
                {
                    currentTile = targetTile;
                    state = EnemyState.DEFAULT;
                }

                break;
            default:
                state = EnemyState.DEFAULT;
                break;
        }
    }

    // TODO: Enemy chases the player when it is nearby
    private void HandleEnemyBehavior2(){
        material.color = Color.magenta;

        switch (state) {
            case EnemyState.DEFAULT:
                material.color = Color.magenta;

                // Check if the player is near the enemy. If so, chase the player
                // If not, randomly move around the maze.
                if (path2.Count <= 0) path2 = pathFinder.RandomPath(currentTile, 20);

                if (path2.Count > 0) {
                    targetTile = path2.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;
            case EnemyState.CHASE:
                // Get the players tile and subtract from the tile to get tile that is behind the player
                Tile playersCurrentTile = playerGameObject.GetComponent<Player>().currentTile;
                Tile targetChaseTile = playersCurrentTile;
                targetChaseTile.indexX -= 1;
                targetChaseTile.indexY -= 1;

                targetTile = targetChaseTile;

                if (path2.Count <= 0) {
                    // Create a path to the player. 
                    // pathFinder.clearASTARLists();
                    path2.Clear();
                    path2 = pathFinder.FindPathAStar(currentTile, targetTile);
                    playeCloseCounter = 0;
                }

                if (path2.Count > 0) targetTile = path2.Dequeue();

                // Go to moving state
                state = EnemyState.MOVING;
                break;

            case EnemyState.MOVING:
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed);

                if (Vector3.Distance(transform.position, targetTile.transform.position) <= speed) {
                    // update current tile
                    currentTile = targetTile;

                    // decrease counter
                    playeCloseCounter--;

                    // Check if the player is close 
                    if (playeCloseCounter <= 0) {

                        targetTile = currentTile;
                        if (Vector3.Distance(transform.position, playerGameObject.transform.position) < visionDistance) {
                            Debug.Log("Player is Close");
                            path2.Clear();
                            playeCloseCounter = maxCount;
                        }
                    }
                    if (playeCloseCounter > 0) {
                        Debug.Log("Chase");
                        state = EnemyState.CHASE;
                    } else state = EnemyState.DEFAULT;
                }
                break;
            default:
                state = EnemyState.DEFAULT;
                break;
        }
        
    }

    // TODO: Third behavior (Describe what it does)
    private void HandleEnemyBehavior3()
    {
        material.color = Color.cyan;

        switch (state) {
            case EnemyState.DEFAULT:

                // Check if the player is near the enemy. If so, chase the player
                // If not, randomly move around the maze.
                if (path3.Count <= 0) path3 = pathFinder.RandomPath(currentTile, 20);

                if (path3.Count > 0) {
                    targetTile = path3.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;
            case EnemyState.CHASE:
                // Get the players tile and subtract from the tile to get tile that is behind the player
                Tile playersCurrentTile = playerGameObject.GetComponent<Player>().currentTile;
                Tile targetChaseTile = playersCurrentTile;
                targetChaseTile.indexX -= 2;
                targetChaseTile.indexY -= 2;

                targetTile = targetChaseTile;

                if (path3.Count <= 0) {
                    // Create a path to the player. 
                    // pathFinder.clearASTARLists();
                    path3.Clear();
                    path3 = pathFinder.FindPathAStar(currentTile, targetTile);
                    playeCloseCounter = 0;
                }

                if (path3.Count > 0) targetTile = path3.Dequeue();

                // Go to moving state
                state = EnemyState.MOVING;
                break;

            case EnemyState.MOVING:
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed);

                if (Vector3.Distance(transform.position, targetTile.transform.position) <= speed) {
                    // update current tile
                    currentTile = targetTile;

                    // decrease counter
                    playeCloseCounter--;

                    // Check if the player is close 
                    if (playeCloseCounter <= 0) {

                        targetTile = currentTile;
                        if (Vector3.Distance(transform.position, playerGameObject.transform.position) < visionDistance) {
                            Debug.Log("Player is Close");
                            path3.Clear();
                            playeCloseCounter = maxCount;
                        }
                    }
                    if (playeCloseCounter > 0) {
                        Debug.Log("Chase");
                        state = EnemyState.CHASE;
                    } else state = EnemyState.DEFAULT;
                }
                break;
            default:
                state = EnemyState.DEFAULT;
                break;
        }
    }
}
