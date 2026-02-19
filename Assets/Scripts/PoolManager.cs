using System.Collections.Generic;
using DamageNumbersPro;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [SerializeField] DamageNumber defaultDamageNumberPrefab;
    // Maps the PREFAB GameObject to its ObjectPool
    Dictionary<GameObject, ObjectPool<GameObject>> _pools = new();
    
    // Maps the SPAWNED INSTANCE ID to the Pool it belongs to
    Dictionary<int, ObjectPool<GameObject>> _activeObjects = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    #region Spawning Logic

    /// <summary>
    /// Generic spawn method. Pass in a component prefab (e.g., enemyPrefab), and it returns that component.
    /// </summary>
    public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        if (!prefab) return null;

        // Fetch the GameObject pool, spawn the object, and get the component
        GameObject obj = Spawn(prefab.gameObject, position, rotation);
        return obj.GetComponent<T>();
    }

    /// <summary>
    /// Base spawn method using GameObjects.
    /// </summary>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!prefab) return null;

        ObjectPool<GameObject> pool = GetOrCreatePool(prefab);
        GameObject obj = pool.Get();
        
        // Track the instance by its GameObject ID
        _activeObjects[obj.GetInstanceID()] = pool;

        obj.transform.SetPositionAndRotation(position, rotation);
        return obj;
    }

    #endregion

    #region Release Logic

    /// <summary>
    /// Generic release method. Accepts any component.
    /// </summary>
    public void Release<T>(T obj) where T : Component
    {
        if (!obj) return;
        Release(obj.gameObject);
    }

    /// <summary>
    /// Base release method using GameObjects.
    /// </summary>
    public void Release(GameObject obj)
    {
        if (!obj) return;

        int id = obj.GetInstanceID();

        if (_activeObjects.TryGetValue(id, out ObjectPool<GameObject> correctPool))
        {
            _activeObjects.Remove(id);
            correctPool.Release(obj);
        }
        else
        {
            Debug.LogWarning($"PoolManager: Trying to release {obj.name} but pool origin was lost. Destroying.");
            Destroy(obj);
        }
    }

    #endregion

    #region Internal Helpers

    private ObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
    {
        if (_pools.TryGetValue(prefab, out var existingPool)) {
            return existingPool;
        }

        var newPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(prefab, transform),
            actionOnGet: (obj) => {
                obj.SetActive(true);
                // Trigger interface if the object uses it
                if (obj.TryGetComponent(out IPooledObject p)) p.OnSpawnFromPool();
            },
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            defaultCapacity: 10, 
            maxSize: 100 // You can also pass these as parameters if you want variable pool sizes per prefab
        );

        _pools.Add(prefab, newPool);
        return newPool;
    }

    #endregion
    
    #region Damage Numbers

    public void SpawnDefaultDamageNumber(Vector3 position, float damage) {
        defaultDamageNumberPrefab.Spawn(position, damage);
    }

    #endregion
}
