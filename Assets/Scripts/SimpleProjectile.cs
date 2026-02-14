
using Enemy;
using UnityEngine;

public class SimpleProjectile : MonoBehaviour
{
    public float lifeTime = 3f;
    public int damage = 10;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        CompositeTarget partHit = other.GetComponent<CompositeTarget>();
        
        if (partHit != null)
        {
            partHit.ReceiveHit(damage);
            Destroy(gameObject);
            return;
        }
        
        if (!other.isTrigger) 
        {
            Destroy(gameObject);
        }
    }
}
