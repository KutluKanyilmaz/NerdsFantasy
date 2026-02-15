using System.Collections.Generic;
using DamageNumbersPro;
using Enemies;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    public ObjectPool<PooledProjectile> projectilePool;
    public ObjectPool<Enemy> enemyPool;
    public ObjectPool<DamageNumber> damageNumberPool;

    [SerializeField] PooledProjectile projectilePrefab;
    [SerializeField] Enemy enemyPrefab;
    [SerializeField] DamageNumber damageNumberPrefab;
    public DamageNumber DamageNumberPrefab { get { return damageNumberPrefab; } }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // --- Projectile Pool ---
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

        // --- Enemy Pool ---
        enemyPool = new ObjectPool<Enemy>(
            createFunc: () => Instantiate(enemyPrefab, transform),
            actionOnGet: (obj) => {
                obj.gameObject.SetActive(true);
                if (obj.TryGetComponent(out IPooledObject p)) p.OnSpawnFromPool();
            },
            actionOnRelease: (obj) => obj.gameObject.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj.gameObject),
            defaultCapacity: 10, maxSize: 50
        );
    }

    // --- Helper Methods ---

    #region Spawning Logic
    public PooledProjectile SpawnProjectile(Vector3 position, Quaternion rotation) {
        if (!projectilePrefab) return null;
        var obj = projectilePool.Get();
        obj.transform.SetPositionAndRotation(position, rotation);
        return obj;
    }

    public Enemy SpawnEnemy(Vector3 position, Quaternion rotation) {
        if (!enemyPrefab) return null;
        var obj = enemyPool.Get();
        obj.transform.SetPositionAndRotation(position, rotation);
        return obj;
    }
    #endregion

    #region Release Logic
    public void ReleaseProjectile(PooledProjectile obj) => projectilePool?.Release(obj);
    public void ReleaseEnemy(Enemy obj) => enemyPool?.Release(obj);
    #endregion
}