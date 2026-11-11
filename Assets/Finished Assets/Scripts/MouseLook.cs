using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("Sensitivity")]
    public float sensitivity = 2f;
 
    [Header("Look Limits (degrees)")]
    public float maxHorizontalAngle = 90f; // left and right from the starting direction
    public float maxVerticalAngle = 80f;   // up and down
 
    private float yaw;   // current left/right rotation
    private float pitch; // current up/down rotation
 
    void Start()
    {
        // Hide and lock the mouse cursor to the game window
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
 
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;
 
        yaw += mouseX;
        pitch -= mouseY;
 
        // Stop the player from turning past the limits
        yaw = Mathf.Clamp(yaw, -maxHorizontalAngle, maxHorizontalAngle);
        pitch = Mathf.Clamp(pitch, -maxVerticalAngle, maxVerticalAngle);
 
        // localRotation keeps the limits relative to the player holder
        transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
