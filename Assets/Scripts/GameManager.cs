using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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



    int mapSizeX = 15;
    int mapSizeY = 15;
    int numRoomsToGenerate = 30;
    int minNumRoomsToReturn = 42;
    int maxNumRoomsToReturn = 58;
    int maxNumAttemptsGenerateFillInGapRooms = 1;
    int minRoomWidth = 4;
    int maxRoomWidth = 12;
    int minRoomHeight = 4;
    int maxRoomHeight = 12;
    float numRandomllyAddedProcentHallways = 13f;

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
}
