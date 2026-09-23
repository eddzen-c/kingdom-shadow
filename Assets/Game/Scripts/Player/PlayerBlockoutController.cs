using UnityEngine;

public class PlayerBlockoutController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float groundCheckDistance = 1.1f;

    private Rigidbody rb;
    private float horizontalInput;
    private bool jumpRequested;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        horizontalInput = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            horizontalInput = -1f;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            horizontalInput = 1f;
        }

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        Vector3 velocity = rb.linearVelocity;

        velocity.x = horizontalInput * moveSpeed;
        velocity.z = 0f;

        rb.linearVelocity = velocity;

        if (jumpRequested)
        {
            Vector3 jumpVelocity = rb.linearVelocity;
            jumpVelocity.y = jumpForce;
            rb.linearVelocity = jumpVelocity;

            jumpRequested = false;
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(
            transform.position,
            Vector3.down,
            groundCheckDistance
        );
    }
}