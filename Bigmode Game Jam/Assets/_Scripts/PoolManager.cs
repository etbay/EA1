using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

// Class that can be used for public references to any object pool if it is added to the dictionary
public class PoolManager : MonoBehaviour
{
    public static PoolManager instance;
    [SerializeField] private GameObject explosionPrefab;
    private Dictionary<string, ObjectPool> poolIdentifier;
    private void Awake()
    {
        poolIdentifier = new Dictionary<string, ObjectPool>();
        var newPool = gameObject.AddComponent<ObjectPool>();
        newPool.GeneratePool(15, explosionPrefab);
        AddPool("Explosives", newPool);
        if (instance == null)
        {
            instance = this;
        }
    }
    public void AddPool(string name, ObjectPool pool)
    {
        poolIdentifier.Add(name, pool);
    }
    public ObjectPool AddPool(string name, GameObject prefab, int count)
    {
        ObjectPool newPool = null;
        if (!poolIdentifier.ContainsKey(name))
        {
            newPool = gameObject.AddComponent<ObjectPool>();
            newPool.GeneratePool(count, prefab);
            AddPool(name, newPool);
        }
        return newPool;
    }
    public ObjectPool GetPool(string name)
    {
        return poolIdentifier[name];
    }
    public bool CheckPool(string name)
    {
        return poolIdentifier.ContainsKey(name);
    }
    public GameObject GetItemFromPool(string name)
    {
        var pool = poolIdentifier[name];
        return pool.RequestAndReturnToPool();
    }
}
