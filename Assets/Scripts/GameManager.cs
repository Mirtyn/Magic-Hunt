using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : ProjectBehaviour
{
    public GameObject NodePrefab;
    public GameObject ProjectilePrefab;

    public LayerMask ProjectileHitLayerMask;
    public const string ENEMY_TAG = "Enemy";
    public const string WALL_TAG = "Wall";

    private void Awake()
    {
        GameManager = this;
    }
}
