using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public enum PoolType
{
    Projectile,
    Enemy
}
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    [System.Serializable]
    public class PoolSetup
    {
        public PoolType type;
        public GameObject prefab;
        public int defaultCapacity = 10;
        public int maxCapacity = 50;
    }

    public List<PoolSetup> poolConfigs;
    Dictionary<PoolType, ObjectPool<GameObject>> _pools = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        foreach (var config in poolConfigs)
        {
            // Capture variables for the lambda
            GameObject prefabRef = config.prefab;
            PoolType typeRef = config.type;

            var pool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(prefabRef, transform),
                actionOnGet: (obj) => {
                    obj.SetActive(true);
                    if (obj.TryGetComponent(out IPooledObject p)) p.OnSpawnFromPool();
                },
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj),
                defaultCapacity: config.defaultCapacity,
                maxSize: config.maxCapacity
            );

            _pools.Add(typeRef, pool);
        }
    }


    public GameObject Spawn(PoolType poolType, Vector3 position, Quaternion rotation) {

        if (!_pools.ContainsKey(poolType)) {

            Debug.LogError($"PoolManager: Pool with key '{poolType}' does not exist.");

            return null;

        }
        
        // Get from the specific pool
        GameObject obj = _pools[poolType].Get();


        // Move it to the desired spot
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        
        return obj;
    }

    public void Release(PoolType type, GameObject obj)
    {
        if (_pools.TryGetValue(type, out var pool))
        {
            pool.Release(obj);
        }
    }
}