using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : ProjectBehaviour
{
    public static EnemyManager Instance { get; private set;}
    [SerializeField] private List<Transform> spawnLocations = new List<Transform>();
    [SerializeField] private float zombieWeight = 1.0f;

    private void Awake()
    {
        Instance = this;

        foreach (Transform location in spawnLocations)
        {
            SpawnEnemy(location.position, GameManager.ZombieSO);
        }
    }

    public void SpawnEnemy(Vector2 pos, EnemySO enemySO)
    {
        GameObject obj = Instantiate(GameManager.EnemyPrefab, pos, Quaternion.identity, this.transform);

        switch (enemySO.EnemyType)
        {
            case EnemyType.Zombie:
                obj.AddComponent<Zombie>();
                break;
        }
    }
}