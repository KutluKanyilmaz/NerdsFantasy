using UnityEngine;
using System.Collections;
using DamageNumbersPro;
using Enemies;
using Sirenix.OdinInspector;

public class PooledProjectile : MonoBehaviour, IPooledObject {
    
    // Internal state variables provided by the shooter
    float maxDistance;
    float damage;
    float baseSpeed;
    LayerMask hitLayers;
    bool useSpeedCurve;
    AnimationCurve speedCurve;

    // Internal tracking variables
    bool isReleased;
    bool hasDamaged;
    Rigidbody rb;
    float distanceTravelled;
    float projectileRadius; 

    void Awake() {
        rb = GetComponent<Rigidbody>();
        if (rb != null) {
            rb.isKinematic = true; 
        }

        if (TryGetComponent(out SphereCollider sphere)) {
            float maxScale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.z));
            projectileRadius = sphere.radius * maxScale;
        } 
        else if (TryGetComponent(out CapsuleCollider capsule)) {
            float maxScale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));
            projectileRadius = capsule.radius * maxScale;
        } 
        else {
            projectileRadius = 0.1f; 
        }
    }

    public void OnSpawnFromPool() {
        isReleased = false;
        hasDamaged = false;
        distanceTravelled = 0f;

        if (rb != null && !rb.isKinematic) {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // Updated to take maxDist instead of lifetime
    public void Initialize(float speed, float dmg, float maxDist, LayerMask layers, bool useCurve, AnimationCurve curve) {
        baseSpeed = speed;
        damage = dmg;
        maxDistance = maxDist;
        hitLayers = layers;
        useSpeedCurve = useCurve;
        speedCurve = curve;
    }

    void Update() {
        if (isReleased) return;

        float currentSpeed = baseSpeed;

        // Evaluate the curve based on the percentage of distance travelled
        if (useSpeedCurve) {
            // Protect against divide-by-zero if maxDistance is accidentally set to 0
            float normalizedDistance = maxDistance > 0f ? Mathf.Clamp01(distanceTravelled / maxDistance) : 0f;
            currentSpeed *= speedCurve.Evaluate(normalizedDistance);
        }

        float distanceThisFrame = currentSpeed * Time.deltaTime;

        if (Physics.SphereCast(transform.position, projectileRadius, transform.forward, out RaycastHit hit, distanceThisFrame, hitLayers, QueryTriggerInteraction.Collide)) {
            transform.position = hit.point;
            ProcessHit(hit.collider, hit.point);
            return; 
        }

        // Move the transform and accumulate the distance
        transform.position += transform.forward * distanceThisFrame;
        distanceTravelled += distanceThisFrame;

        // Check if we've reached or exceeded the max distance
        if (distanceTravelled >= maxDistance) {
            ReturnToPool();
        }
    }

    void ProcessHit(Collider other, Vector3 hitPoint) {
        if (isReleased) return;
        
        CompositeTarget partHit = other.GetComponent<CompositeTarget>();
        
        if (partHit != null) {
            if (!hasDamaged) {
                hasDamaged = true;
                PoolManager.Instance.SpawnDefaultDamageNumber(hitPoint, damage * partHit.damageMultiplier);
                partHit.ReceiveHit(damage);
            }
            ReturnToPool();
            return;
        }
        
        if (!other.isTrigger) {
            ReturnToPool();
        }
    }

    void OnTriggerEnter(Collider other) {
        ProcessHit(other, transform.position); 
    }
    
    void ReturnToPool() {
        if (isReleased) return;
        
        isReleased = true;
        PoolManager.Instance.Release(this);
    }
}
