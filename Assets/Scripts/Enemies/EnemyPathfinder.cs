using UnityEngine;
using UnityEngine.AI;
namespace Enemies {

    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyPathfinder : MonoBehaviour
    {
        NavMeshAgent agent;
        Transform playerTarget;

        [Header("Settings")]
        public float stopDistance = 1.5f;

        void Start() {
            SetPlayerAsTarget();
        }

        public void SetPlayerAsTarget() {
            agent = GetComponent<NavMeshAgent>();

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
            
                agent.SetDestination(playerTarget.position);
            }
            else
            {
                Debug.LogError("Enemy could not find an object tagged 'Player'!");
            }

            agent.stoppingDistance = stopDistance;
        }

        void Update()
        {
            /*if (playerTarget != null)
            {
                agent.SetDestination(playerTarget.position);
            }*/
        }
    }
}