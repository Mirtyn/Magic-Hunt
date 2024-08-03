using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler
{
    private GameObject objectPrefab;
    private int pooledAmount;
    private bool willGrow;
    private Transform parent;
    private List<GameObject> pooledObjects = new List<GameObject>();

    public ObjectPooler(GameObject objectPrefab, int amountToPool, bool willGrow, Transform parent)
    {
        this.objectPrefab = objectPrefab;
        pooledAmount = amountToPool;
        this.willGrow = willGrow;
        this.parent = parent;
        Init();
    }

    private void Init()
    {
        for (int i = 0; i < pooledAmount; i++)
        {
            GameObject obj = Object.Instantiate(objectPrefab, parent);
            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        foreach (GameObject i in pooledObjects)
        {
            if (!i.activeInHierarchy)
            {
                return i;
            }
        }

        if (willGrow)
        {
            GameObject obj = Object.Instantiate(objectPrefab, parent);
            pooledObjects.Add(obj);
            return obj;
        }

        return null;
    }

    public GameObject GetObjectPrefab()
    {
        return objectPrefab;
    }

    public void SetObjectPrefab(GameObject prefab)
    {
        objectPrefab = prefab;
    }

    public int GetPooledAmount()
    {
        return pooledAmount;
    }

    public void SetObjectPrefab(int amount)
    {
        pooledAmount = amount;
    }

    public bool GetWillGrow()
    {
        return willGrow;
    }

    public void SetWillGrow(bool willGrow)
    {
        this.willGrow = willGrow;
    }

    public Transform GetParent()
    {
        return parent;
    }

    public void SetParent(Transform parent)
    {
        this.parent = parent;
    }

    public List<GameObject> GetPooledObjects()
    {
        return pooledObjects;
    }
}
