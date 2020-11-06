using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapGen;

//FSM States for the Player
public enum PlayerState {DEFAULT, MOVING, EVADE, GOAL_REACHED, DEAD };

public class Player : MonoBehaviour
{
    PathFinder pathFinder;
    public GenerateMap mapGenerator;
    public Queue<Tile> path;
    public Tile currentTile;
    public Tile targetTile;
    Vector3 velocity;

    //properties
    public float slowSpeed = 0.04f;
    public float fastSpeed = 0.08f;
    float speed = 0.04f;
    int enemyCloseCounter = 0;
    public int maxCounter = 5;
    public float visionDistance = 4;
    Material material;
    Color playerColor;

    PlayerState state = PlayerState.DEFAULT;

    //environment
    List<Enemy> enemyList;
    Enemy closestEnemy;

    //Explosion Effect
    ParticleSystem explosion;
    bool explosionStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        path = new Queue<Tile>();
        pathFinder = new PathFinder();
        enemyList = new List<Enemy>(GameObject.FindObjectsOfType<Enemy>());
        material = GetComponent<MeshRenderer>().material;
        playerColor = material.color;
        currentTile = mapGenerator.start;
        // targetTile = mapGenerator.goal;
        explosion = GameObject.Find("Explosion").GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (mapGenerator.state == MapState.DESTROYED) return;

        HandlePlayerFSMStates();
    }

    public bool IsGoalReached()
    {
        if (state == PlayerState.GOAL_REACHED) return true;
        else return false;
    }

    public bool IsPlayerDead()
    {
        if (state == PlayerState.DEAD) return true;
        else return false;
    }

    private void HandlePlayerFSMStates()
    {
        switch(state)
        {
            case PlayerState.DEFAULT: // Find Path to Goal
                material.color = playerColor;
                speed = slowSpeed;

                if (path.Count <= 0) path = pathFinder.FindPathAStar(currentTile, mapGenerator.goal);
                //if there is a path get next step then update target and go to moving state
                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = PlayerState.MOVING;
                }
                break;
            case PlayerState.EVADE: // if possible find path to goal evading enemies otherwise choose next target based on evade behavior
                speed = fastSpeed;
                material.color = Color.yellow;

                if (path.Count <= 0)
                {
                    path = pathFinder.FindPathAStarEvadeEnemy(currentTile, mapGenerator.goal);
                    enemyCloseCounter = 0;
                }

                if (path.Count > 0) targetTile = path.Dequeue();
                targetTile = FindEvadeTile(closestEnemy.gameObject);
                state = PlayerState.MOVING;
                break;
            case PlayerState.MOVING: //move to next target
                //move
                velocity = targetTile.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed);
                
                // Check if player  is dead
                foreach (Enemy enemy in enemyList)
                {
                    if (Vector3.Distance(enemy.gameObject.transform.position, transform.position) < 0.5f)
                    {
                        state = PlayerState.DEAD;
                    }
                }

                //if target tile is reached
                if (Vector3.Distance(transform.position, targetTile.transform.position) <= speed)
                {
                    //update current tile
                    currentTile = targetTile;
                    //decrease counter
                    enemyCloseCounter--;

                    //if it goal change state to GOAL_REACHED
                    if (currentTile == mapGenerator.goal)
                    {
                        state = PlayerState.GOAL_REACHED;
                        break;
                    }

                    //if counter over check for close enemies
                    if (enemyCloseCounter <= 0)
                    {
                        targetTile = currentTile;
                        foreach (Enemy enemy in enemyList)
                        {
                            if (Vector3.Distance(enemy.gameObject.transform.position, transform.position) < visionDistance)
                            {
                                closestEnemy = enemy;
                                path.Clear();
                                //if an enemy is close reset counter
                                enemyCloseCounter = maxCounter;
                                break;

                            }
                        }
                    }
                    //if counter is over 0 got to evade
                    if (enemyCloseCounter > 0) state = PlayerState.EVADE;
                    else state = PlayerState.DEFAULT;
                }
                break;
            case PlayerState.GOAL_REACHED:
                material.color = playerColor;
                break;
            case PlayerState.DEAD:
                Debug.Log("Player Dead");
                transform.gameObject.GetComponent<Renderer>().enabled = false;
                StartExplosion();
                break;
            default:
                state = PlayerState.DEFAULT;
                break;
        }
    }

    // Find a tile to evade from an incomming enemy
    // Lookahead time is a fixed value but could be estimated as well
    private Tile FindEvadeTile(GameObject enemy)
    {
        Tile nextTile = null;
        //Debug.Log("Finding Evade Tile");

        //Predict target location
        Vector3 targetVelocity = enemy.GetComponent<Enemy>().velocity;
        float lookaheadTime = 50;
        Vector3 targetPredictedPosition = enemy.transform.position + targetVelocity * lookaheadTime;

        //Evade from the target: FInd tile in opposite direction
        double maxangle = 0;
        foreach(Tile adjacent in currentTile.Adjacents)
        {
            Vector3 adjacentDirection = adjacent.transform.position - transform.position;
            Vector3 targetDirection = targetPredictedPosition - transform.position;
            double angle = Mathf.Acos(Vector3.Dot(adjacentDirection.normalized,targetDirection.normalized));
            if(angle > maxangle)
            {
                nextTile = adjacent;
                maxangle = angle;
            }
        }
        return nextTile;
    }

    private void StartExplosion()
    {
        if(explosionStarted == false)
        {
            explosion.Play();
            explosionStarted = true;
        }
        
    }
    private void StopExplosion()
    {
        explosionStarted = false;
        explosion.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
    }

   
    public void Reset(Tile tile)
    {
        Debug.Log("Player reset");
        path.Clear();
        pathFinder.clearASTARLists();
        transform.gameObject.GetComponent<Renderer>().enabled = true;
        StopExplosion();
        state = PlayerState.DEFAULT;
        currentTile = tile;
    }
}
