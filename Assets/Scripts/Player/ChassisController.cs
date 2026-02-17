using UnityEngine;
using UnityEngine.InputSystem;

namespace Player 
{
    public class ChassisController : MonoBehaviour
    {
        public float rotationSpeed = 100f;
        
        [SerializeField] InputActionReference moveAction;

        void Update()
        {
            // ReadValue<Vector2> handles both WASD and Joysticks. 
            // We only need the X (horizontal) value for turning.
            float turn = moveAction.action.ReadValue<Vector2>().x;
        
            if (Mathf.Abs(turn) > 0.01f)
            {
                transform.Rotate(Vector3.up, turn * rotationSpeed * Time.deltaTime);
            }
        }
    }
}