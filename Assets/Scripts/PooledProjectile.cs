using UnityEngine;
using System.Collections;
using Enemy;

public class PooledProjectile : MonoBehaviour, IPooledObject
{
    [Header("Settings")]
    public string myPoolKey = "Bullet";
    public float lifetime = 2f;
    public float damage = 10f;
    
    bool isReleased;
    bool hasDamaged;
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnSpawnFromPool()
    {
        isReleased = false;
        hasDamaged = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        StartCoroutine(DeactivateRoutine());
    }

    IEnumerator DeactivateRoutine()
    {
        yield return new WaitForSeconds(lifetime);
        
        // Return myself to the manager
        PoolManager.Instance.Release(myPoolKey, gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        CompositeTarget partHit = other.GetComponent<CompositeTarget>();
        
        if (partHit != null)
        {
            if (!hasDamaged)
            {
                partHit.ReceiveHit(damage);
            }
            ReturnToPool();
            return;
        }
        
        if (!other.isTrigger) 
        {
            ReturnToPool();
        }
    }
    
    void ReturnToPool()
    {
        if (isReleased) return;
        
        isReleased = true;
        StopAllCoroutines();
        PoolManager.Instance.Release(myPoolKey, gameObject);
    }
}