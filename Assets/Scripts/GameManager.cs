using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : ProjectBehaviour
{
    public GameObject NodePrefab;
    public GameObject ProjectilePrefab;
    public GameObject EnemyPrefab;
    public GameObject TilePrefab;

    public EnemySO ZombieSO;

    public LayerMask ProjectileHitLayerMask;
    public LayerMask PlayerLayerMask;
    public const string ENEMY_TAG = "Enemy";
    public const string WALL_TAG = "Wall";
    public const string PLAYER_TAG = "Player";



    int mapSizeX = 50;
    int mapSizeY = 50;
    int numRoomsToGenerate = 150;
    int minNumRoomsToReturn = 42;
    int maxNumRoomsToReturn = 58;
    int maxNumAttemptsGenerateFillInGapRooms = 1;
    int minRoomWidth = 5;
    int maxRoomWidth = 24;
    int minRoomHeight = 5;
    int maxRoomHeight = 24;
    float numRandomllyAddedProcentHallways = 10f;

    private void Awake()
    {
        GameManager = this;

        DungeonGenerator dungeonGenerator = new DungeonGenerator
        (
            mapSizeX,
            mapSizeY,
            numRoomsToGenerate,
            minNumRoomsToReturn,
            maxNumRoomsToReturn,
            maxNumAttemptsGenerateFillInGapRooms,
            minRoomWidth,
            maxRoomWidth,
            minRoomHeight,
            maxRoomHeight,
            numRandomllyAddedProcentHallways
        );

        StartCoroutine(dungeonGenerator.Start());
    }

    private void Start()
    {
        AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync("SimulatedScene");
    }
}
