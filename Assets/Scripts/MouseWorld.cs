using UnityEngine;
using UnityEngine.InputSystem;

public class MouseWorld : MonoBehaviour
{
    public static MouseWorld Instance;

    [Header("Settings")]
    [Tooltip("Layers the mouse raycast should hit (e.g., Ground, Enemies).")]
    public LayerMask mousePlaneLayerMask;

    [Header("Visuals")]
    [Tooltip("Drag a visual object here (e.g., a small red sphere) to show where the mouse is in 3D.")]
    public Transform visualCursor; 

    Camera mainCamera;

    void Awake() 
    {
        if (Instance == null) 
        {
            Instance = this;
        }
        else 
        {
            Destroy(gameObject);
        }
        
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Move the visual cursor to the mouse position every frame
        if (visualCursor != null)
        {
            visualCursor.position = GetPosition();
        }
    }

    // Call this from your Player script (or any other script)
    public Vector3 GetPosition() 
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        // Raycast against the specified layers (Ground + Enemy)
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 999f, mousePlaneLayerMask)) 
        {
            return hitInfo.point;
        }
        
        // Fallback: If we aim at the void, return a point far away or the last known position
        return Vector3.zero;
    }
}
