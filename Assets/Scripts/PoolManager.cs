using System.Collections.Generic;
using DamageNumbersPro;
using Enemies;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    // --- Projectiles (Kept as is) ---
    public ObjectPool<PooledProjectile> projectilePool;
    [SerializeField] PooledProjectile projectilePrefab;

    // --- Damage Numbers (Kept as is) ---
    public ObjectPool<DamageNumber> damageNumberPool;
    [SerializeField] DamageNumber damageNumberPrefab;
    public DamageNumber DamageNumberPrefab { get { return damageNumberPrefab; } }

    // --- ENEMIES (Dynamic Multi-Pool System) ---
    // Maps the PREFAB to its specific ObjectPool
    private Dictionary<GameObject, ObjectPool<Enemy>> enemyPools = new Dictionary<GameObject, ObjectPool<Enemy>>();
    
    // Maps the SPAWNED INSTANCE ID to the Pool it belongs to (so we know where to return it)
    private Dictionary<int, ObjectPool<Enemy>> activeEnemyIds = new Dictionary<int, ObjectPool<Enemy>>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // --- Projectile Pool Init ---
        projectilePool = new ObjectPool<PooledProjectile>(
            createFunc: () => Instantiate(projectilePrefab, transform),
            actionOnGet: (obj) => {
                obj.gameObject.SetActive(true);
                if (obj.TryGetComponent(out IPooledObject p)) p.OnSpawnFromPool();
            },
            actionOnRelease: (obj) => obj.gameObject.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj.gameObject),
            defaultCapacity: 50, maxSize: 200
        );
        
        // Note: Enemy pools are now created lazily in SpawnEnemy
    }

    #region Spawning Logic

    public PooledProjectile SpawnProjectile(Vector3 position, Quaternion rotation) {
        if (!projectilePrefab) return null;
        var obj = projectilePool.Get();
        obj.transform.SetPositionAndRotation(position, rotation);
        return obj;
    }

    // UPDATED: Now requires the specific prefab to spawn
    public Enemy SpawnEnemy(GameObject prefab, Vector3 position, Quaternion rotation) {
        if (!prefab) return null;

        // 1. Get or Create the specific pool for this prefab
        ObjectPool<Enemy> pool = GetOrCreateEnemyPool(prefab);

        // 2. Get object
        var obj = pool.Get();
        
        // 3. Track which pool this specific instance belongs to
        activeEnemyIds[obj.GetInstanceID()] = pool;

        obj.transform.SetPositionAndRotation(position, rotation);
        return obj;
    }

    #endregion

    #region Release Logic

    public void ReleaseProjectile(PooledProjectile obj) => projectilePool?.Release(obj);

    // UPDATED: Looks up where this enemy came from
    public void ReleaseEnemy(Enemy obj) {
        int id = obj.GetInstanceID();

        // Check if we are tracking this enemy
        if (activeEnemyIds.TryGetValue(id, out ObjectPool<Enemy> correctPool)) {
            // Remove from tracker
            activeEnemyIds.Remove(id);
            // Return to its specific pool
            correctPool.Release(obj);
        }
        else {
            // Fallback if something went wrong (e.g. scene reload cleared dictionary)
            Debug.LogWarning($"PoolManager: Trying to release enemy {obj.name} but pool origin was lost. Destroying.");
            Destroy(obj.gameObject);
        }
    }

    #endregion

    // --- Internal Helpers ---

    ObjectPool<Enemy> GetOrCreateEnemyPool(GameObject prefab)
    {
        // If pool exists, return it
        if (enemyPools.TryGetValue(prefab, out var existingPool)) {
            return existingPool;
        }

        // Otherwise, create a new pool logic
        var newPool = new ObjectPool<Enemy>(
            createFunc: () => {
                // IMPORTANT: Instantiate the specific prefab passed in
                var instance = Instantiate(prefab, transform).GetComponent<Enemy>();
                if (instance == null) Debug.LogError($"Prefab {prefab.name} does not have an Enemy component!");
                return instance;
            },
            actionOnGet: (obj) => {
                obj.gameObject.SetActive(true);
                if (obj.TryGetComponent(out IPooledObject p)) p.OnSpawnFromPool();
            },
            actionOnRelease: (obj) => obj.gameObject.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj.gameObject),
            defaultCapacity: 10, 
            maxSize: 50
        );

        // Store and return
        enemyPools.Add(prefab, newPool);
        return newPool;
    }
}