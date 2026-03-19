using UnityEngine;

public class TimmyBehavior : MonoBehaviour
{

    [Header("Arena Center + Size")]
    public Vector3 arenaCenter = Vector3.zero;
    public Vector2 arenaSize = new Vector2(16f, 10f); // X width, Z height

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float acceleration = 8f;
    public float turnSpeed = 8f;
    public float arriveDistance = 0.4f;

    [Header("Wander Timing")]
    public float minPauseTime = 0.2f;
    public float maxPauseTime = 0.8f;
    public float minMoveTime = 0.5f;
    public float maxMoveTime = 1.5f;

    [Header("Center Bias")]
    [Tooltip("Higher = stronger tendency to pick positions near center.")]
    public float centerBias = 2.5f;

    [Tooltip("Extra push away from borders when close to edges.")]
    public float edgeAvoidStrength = 3f;

    [Tooltip("Distance from edge where border avoidance starts.")]
    public float edgeAvoidDistance = 1.5f;

    [Header("Curved Motion")]
    [Tooltip("How much random side drift is added while moving.")]
    public float curveStrength = 0.8f;

    [Tooltip("How often the curve direction changes.")]
    public float curveChangeInterval = 0.6f;

    private Rigidbody rb;
    private Vector3 targetPoint;
    private float stateTimer;
    private float curveTimer;
    private float currentCurveOffset;
    private bool isPausing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Start()
    {
        PickNewPause();
    }

    private void FixedUpdate()
    {
        stateTimer -= Time.fixedDeltaTime;

        if (isPausing)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, acceleration * Time.fixedDeltaTime);

            if (stateTimer <= 0f)
            {
                PickNewTarget();
            }

            FaceMovementDirection();
            return;
        }

        curveTimer -= Time.fixedDeltaTime;
        if (curveTimer <= 0f)
        {
            curveTimer = curveChangeInterval;
            currentCurveOffset = Random.Range(-curveStrength, curveStrength);
        }

        Vector3 toTarget = targetPoint - transform.position;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;

        if (distance <= arriveDistance || stateTimer <= 0f)
        {
            PickNewPause();
            return;
        }

        Vector3 desiredDir = toTarget.normalized;

        // Add subtle sideways drift for curved movement.
        Vector3 side = Vector3.Cross(Vector3.up, desiredDir);
        desiredDir += side * currentCurveOffset;

        // Push away from edges if near them.
        desiredDir += GetEdgeAvoidance();

        desiredDir.y = 0f;
        desiredDir = desiredDir.normalized;

        Vector3 desiredVelocity = desiredDir * moveSpeed;
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, desiredVelocity, acceleration * Time.fixedDeltaTime);

        FaceMovementDirection();
        ClampInsideArena();
    }

    private void PickNewTarget()
    {
        isPausing = false;
        stateTimer = Random.Range(minMoveTime, maxMoveTime);
        curveTimer = 0f;

        targetPoint = GetCenterBiasedPoint();
    }

    private void PickNewPause()
    {
        isPausing = true;
        stateTimer = Random.Range(minPauseTime, maxPauseTime);
        currentCurveOffset = 0f;
    }

    private Vector3 GetCenterBiasedPoint()
    {
        float halfX = arenaSize.x * 0.5f;
        float halfZ = arenaSize.y * 0.5f;

        // Random.value is 0..1. Raising it creates a center bias.
        float rx = GetBiasedAxisOffset(halfX);
        float rz = GetBiasedAxisOffset(halfZ);

        return new Vector3(
            arenaCenter.x + rx,
            transform.position.y,
            arenaCenter.z + rz
        );
    }

    private float GetBiasedAxisOffset(float halfExtent)
    {
        float sign = Random.value < 0.5f ? -1f : 1f;

        // Stronger center bias:
        // value^(centerBias) makes most picks closer to 0.
        float t = Mathf.Pow(Random.value, centerBias);

        return sign * t * halfExtent;
    }

    private Vector3 GetEdgeAvoidance()
    {
        float halfX = arenaSize.x * 0.5f;
        float halfZ = arenaSize.y * 0.5f;

        Vector3 local = transform.position - arenaCenter;
        Vector3 push = Vector3.zero;

        float distRight = halfX - local.x;
        float distLeft = halfX + local.x;
        float distTop = halfZ - local.z;
        float distBottom = halfZ + local.z;

        if (distRight < edgeAvoidDistance)
            push += Vector3.left * ((edgeAvoidDistance - distRight) / edgeAvoidDistance);

        if (distLeft < edgeAvoidDistance)
            push += Vector3.right * ((edgeAvoidDistance - distLeft) / edgeAvoidDistance);

        if (distTop < edgeAvoidDistance)
            push += Vector3.back * ((edgeAvoidDistance - distTop) / edgeAvoidDistance);

        if (distBottom < edgeAvoidDistance)
            push += Vector3.forward * ((edgeAvoidDistance - distBottom) / edgeAvoidDistance);

        return push * edgeAvoidStrength;
    }

    private void ClampInsideArena()
    {
        float halfX = arenaSize.x * 0.5f;
        float halfZ = arenaSize.y * 0.5f;

        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, arenaCenter.x - halfX, arenaCenter.x + halfX);
        pos.z = Mathf.Clamp(pos.z, arenaCenter.z - halfZ, arenaCenter.z + halfZ);

        rb.MovePosition(pos);
    }

    private void FaceMovementDirection()
    {
        Vector3 flatVelocity = rb.linearVelocity;
        flatVelocity.y = 0f;

        if (flatVelocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatVelocity.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 size = new Vector3(arenaSize.x, 0.1f, arenaSize.y);
        Gizmos.DrawWireCube(arenaCenter, size);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetPoint, 0.25f);
    }

}
