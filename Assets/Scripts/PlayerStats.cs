using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : ProjectBehaviour
{
    public static PlayerStats Instance { get; private set; }
    private Transform thisTransform;
    private Vector2 position;

    private float health = 20f;
    private float spawnProjectilesDistance = 0.8f;
    private float moveSpeed = 5f;
    //private float rotateSpeed = 90f;
    private float playerRadius = 0.4f;
    private float accelerateSpeed = 25f;

    private void Awake()
    {
        Instance = this;
        thisTransform = transform;
        position = thisTransform.position;
    }

    private void Update()
    {
        position = thisTransform.position;
    }

    public void GetAttacked(float damage)
    {
        health -= damage;
    }

    public float GetSpawnProjectilesDistance()
    {
        return spawnProjectilesDistance;
    }

    public float GetHealth()
    {
        return health;
    }

    public Transform GetTransform()
    {
        return thisTransform;
    }

    public Vector2 GetPositionVector2()
    {
        return position;
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public float GetPlayerRadius()
    {
        return playerRadius;
    }

    public float GetAccelerateSpeed()
    {
        return accelerateSpeed;
    }
}
