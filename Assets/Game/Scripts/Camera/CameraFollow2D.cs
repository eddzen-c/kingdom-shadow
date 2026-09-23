using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target;

    [Header("Seguimiento")]
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private bool followVertical = false;

    [Header("Anticipación")]
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSmoothTime = 0.2f;

    [Header("Límites horizontales")]
    [SerializeField] private float minX = 0f;
    [SerializeField] private float maxX = 100f;

    private Vector3 cameraVelocity;
    private float lookAheadVelocity;
    private float currentLookAhead;

    private float offsetX;
    private float offsetY;
    private float previousTargetX;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("CameraFollow2D necesita un Target.");
            enabled = false;
            return;
        }

        offsetX = transform.position.x - target.position.x;
        offsetY = transform.position.y - target.position.y;

        previousTargetX = target.position.x;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        float movement = target.position.x - previousTargetX;

        float desiredLookAhead = 0f;

        if (movement > 0.001f)
        {
            desiredLookAhead = lookAheadDistance;
        }
        else if (movement < -0.001f)
        {
            desiredLookAhead = -lookAheadDistance;
        }

        currentLookAhead = Mathf.SmoothDamp(
            currentLookAhead,
            desiredLookAhead,
            ref lookAheadVelocity,
            lookAheadSmoothTime
        );

        float targetX =
            target.position.x
            + offsetX
            + currentLookAhead;

        targetX = Mathf.Clamp(
            targetX,
            minX,
            maxX
        );

        float targetY = followVertical
            ? target.position.y + offsetY
            : transform.position.y;

        Vector3 desiredPosition = new Vector3(
            targetX,
            targetY,
            transform.position.z
        );

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref cameraVelocity,
            smoothTime
        );

        previousTargetX = target.position.x;
    }
}