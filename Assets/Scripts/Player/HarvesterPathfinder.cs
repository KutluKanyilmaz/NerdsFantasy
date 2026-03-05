using UnityEngine;
using UnityEngine.AI;

namespace Player 
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class HarvesterPathfinder : MonoBehaviour
    {
        NavMeshAgent agent;
        Transform currentTarget;

        void Awake() 
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public void SetSpeed(float speed)
        {
            if (agent != null) agent.speed = speed;
        }

        public void SetTarget(Transform target) 
        {
            currentTarget = target;
            
            if (agent != null && currentTarget != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(currentTarget.position);
            }
        }

        public void ClearTarget()
        {
            currentTarget = null;
            
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }
        }

        void Update()
        {
            // Optional: If you ever make crystals that move, this ensures the droid tracks them smoothly
            if (currentTarget != null && agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(currentTarget.position);
            }
        }
    }
}