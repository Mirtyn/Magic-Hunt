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

    private void Awake()
    {
        GameManager = this;

        DungeonGenerator dungeonGenerator = new DungeonGenerator(20, 20, 100, 25, 35, 100, 2, 10, 2, 10);
        dungeonGenerator.Start();
    }
}
