using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;
    public float gravity = -20f;

    private CharacterController controller;
    private float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal"); // A / D
        float z = Input.GetAxis("Vertical");   // W / S

        Vector3 move = (transform.right * x + transform.forward * z) * speed;

        // Gravity
        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }
}