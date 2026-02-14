using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    [System.Serializable]
    public class PoolSetup
    {
        public string poolKey;
        public GameObject prefab;
        public int defaultCapacity = 10;
        public int maxCapacity = 50;
    }

    [Header("Configuration")]
    [Tooltip("Define your pools here. The 'Pool Key' is what you use to spawn them.")]
    public List<PoolSetup> poolsToCheck;

    // The runtime dictionary storing the actual logic
    Dictionary<string, ObjectPool<GameObject>> _poolDictionary;

    void Awake()
    {
        // Singleton Setup
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _poolDictionary = new Dictionary<string, ObjectPool<GameObject>>();

        // Initialize every pool defined in the Inspector
        foreach (var config in poolsToCheck)
        {
            // We create a local variable for the prefab to avoid closure issues in the lambda
            GameObject prefabRef = config.prefab;
            string keyRef = config.poolKey;

            var newPool = new ObjectPool<GameObject>(
                createFunc: () => 
                {
                    // 1. Create Logic: Instantiate and parent to this manager to keep hierarchy clean
                    GameObject obj = Instantiate(prefabRef, transform);
                    obj.name = keyRef; // Optional: helps debug
                    return obj;
                },
                actionOnGet: (obj) => 
                {
                    // 2. Get Logic: Activate and reset
                    obj.SetActive(true);
                    
                    // Trigger the custom "OnSpawn" logic if the object has the interface
                    var pooledScript = obj.GetComponent<IPooledObject>();
                    if (pooledScript != null) pooledScript.OnSpawnFromPool();
                },
                actionOnRelease: (obj) => 
                {
                    // 3. Release Logic: Deactivate
                    obj.SetActive(false);
                },
                actionOnDestroy: (obj) => 
                {
                    // 4. Destroy Logic: Actual destruction if pool is full
                    Destroy(obj);
                },
                collectionCheck: true,
                defaultCapacity: config.defaultCapacity,
                maxSize: config.maxCapacity
            );

            _poolDictionary.Add(config.poolKey, newPool);
        }
    }

    // --- Public API ---

    public GameObject Spawn(string poolKey, Vector3 position, Quaternion rotation)
    {
        if (!_poolDictionary.ContainsKey(poolKey))
        {
            Debug.LogError($"PoolManager: Pool with key '{poolKey}' does not exist.");
            return null;
        }

        // Get from the specific pool
        GameObject obj = _poolDictionary[poolKey].Get();
        
        // Move it to the desired spot
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        return obj;
    }

    public void Release(string poolKey, GameObject obj)
    {
        if (!_poolDictionary.ContainsKey(poolKey))
        {
            Debug.LogError($"PoolManager: Trying to release to unknown pool '{poolKey}'");
            return;
        }

        _poolDictionary[poolKey].Release(obj);
    }
}