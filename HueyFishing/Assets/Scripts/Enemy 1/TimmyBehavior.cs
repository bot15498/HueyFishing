using UnityEngine;

public class TimmyBehavior : FishMovement
{

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public bool faceRightByDefault = true;
    public float flipThreshold = 0.05f;

    [Header("Arena Center + Size")]
    
    public Vector3 arenaCenter;
    public Vector2 arenaSize = new Vector2(16f, 10f);

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float acceleration = 8f;
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

    [Header("Collider Avoidance")]
    [Tooltip("How far ahead the fish checks for colliders.")]
    public float avoidanceCheckDistance = 1.5f;

    [Tooltip("Width of the avoidance check.")]
    public float avoidanceRadius = 0.35f;

    [Tooltip("How strongly the fish steers away from colliders.")]
    public float avoidanceStrength = 4f;

    [Tooltip("How far left/right the side feelers angle out.")]
    [Range(5f, 80f)]
    public float sideFeelerAngle = 35f;

    [Tooltip("Optional layer mask for obstacles.")]
    public LayerMask obstacleMask = ~0;

    [Tooltip("Ignore trigger colliders during avoidance.")]
    public bool ignoreTriggers = true;

    [Header("Stuck Recovery")]
    [Tooltip("If speed stays below this, fish may be considered stuck.")]
    public float stuckSpeedThreshold = 0.15f;

    [Tooltip("How long the fish must be barely moving before escape kicks in.")]
    public float stuckTimeThreshold = 0.4f;

    [Tooltip("How long escape steering lasts.")]
    public float escapeDuration = 0.5f;

    [Tooltip("How strongly the fish pushes during escape.")]
    public float escapeStrength = 6f;

    private Rigidbody rb;
    private Collider[] ownColliders;
    private Vector3 targetPoint;
    private float stateTimer;
    private float curveTimer;
    private float currentCurveOffset;
    private bool isPausing;

    private float stuckTimer;
    private float escapeTimer;
    private Vector3 escapeDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        ownColliders = GetComponentsInChildren<Collider>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (fishSpawnCenter != null)
            arenaCenter = fishSpawnCenter.transform.position;

        PickNewPause();
        rb.constraints = RigidbodyConstraints.FreezeRotation
               | RigidbodyConstraints.FreezePositionY;
    }

    private void FixedUpdate()
    {
        if (fishSpawnCenter != null)
            arenaCenter = fishSpawnCenter.transform.position;

        stateTimer -= Time.fixedDeltaTime;

        if (isPausing)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity,Vector3.zero,acceleration * Time.fixedDeltaTime);

            if (stateTimer <= 0f)
            {
                PickNewTarget();
            }

            UpdateSpriteFlip();
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
            UpdateSpriteFlip();
            return;
        }

        Vector3 desiredDir = toTarget.normalized;

        // Curved drift
        Vector3 side = Vector3.Cross(Vector3.up, desiredDir);
        desiredDir += side * currentCurveOffset;

        // Arena edge avoidance
        desiredDir += GetEdgeAvoidance();

        // Better collider avoidance
        desiredDir += GetColliderAvoidance(desiredDir);

        // Stuck escape
        UpdateStuckState(desiredDir);
        if (escapeTimer > 0f)
        {
            desiredDir += escapeDirection * escapeStrength;
            escapeTimer -= Time.fixedDeltaTime;
        }

        desiredDir.y = 0f;
        desiredDir = desiredDir.sqrMagnitude > 0.0001f ? desiredDir.normalized : Vector3.zero;

        Vector3 desiredVelocity = desiredDir * moveSpeed;
        rb.linearVelocity = Vector3.Lerp(
            rb.linearVelocity,
            desiredVelocity,
            acceleration * Time.fixedDeltaTime
        );

        ClampInsideArena();
        UpdateSpriteFlip();
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
        stuckTimer = 0f;
        escapeTimer = 0f;
    }

    private Vector3 GetCenterBiasedPoint()
    {
        float halfX = arenaSize.x * 0.5f;
        float halfZ = arenaSize.y * 0.5f;

        float rx = GetBiasedAxisOffset(halfX);
        float rz = GetBiasedAxisOffset(halfZ);

        return new Vector3( arenaCenter.x + rx, transform.position.y, arenaCenter.z + rz);
    }

    private float GetBiasedAxisOffset(float halfExtent)
    {
        float sign = Random.value < 0.5f ? -1f : 1f;
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

    private Vector3 GetColliderAvoidance(Vector3 desiredDir)
    {
        Vector3 castDir = rb.linearVelocity.sqrMagnitude > 0.05f
            ? rb.linearVelocity.normalized
            : desiredDir.normalized;

        castDir.y = 0f;

        if (castDir.sqrMagnitude <= 0.0001f)
            return Vector3.zero;

        QueryTriggerInteraction triggerMode = ignoreTriggers
            ? QueryTriggerInteraction.Ignore
            : QueryTriggerInteraction.Collide;

        Vector3 origin = transform.position + Vector3.up * 0.1f;

        Vector3 leftDir = Quaternion.AngleAxis(-sideFeelerAngle, Vector3.up) * castDir;
        Vector3 rightDir = Quaternion.AngleAxis(sideFeelerAngle, Vector3.up) * castDir;

        bool hitForward = CastObstacle(origin, castDir, out RaycastHit forwardHit, triggerMode);
        bool hitLeft = CastObstacle(origin, leftDir, out RaycastHit leftHit, triggerMode);
        bool hitRight = CastObstacle(origin, rightDir, out RaycastHit rightHit, triggerMode);

        // Nothing ahead
        if (!hitForward && !hitLeft && !hitRight)
            return Vector3.zero;

        Vector3 avoid = Vector3.zero;

        // Main push from forward obstacle normal
        if (hitForward)
        {
            Vector3 normalPush = forwardHit.normal;
            normalPush.y = 0f;

            float closeness = 1f - (forwardHit.distance / avoidanceCheckDistance);
            avoid += normalPush.normalized * closeness;
        }

        // Prefer the more open side
        float leftClear = hitLeft ? leftHit.distance : avoidanceCheckDistance;
        float rightClear = hitRight ? rightHit.distance : avoidanceCheckDistance;

        if (leftClear > rightClear)
            avoid += leftDir.normalized;
        else if (rightClear > leftClear)
            avoid += rightDir.normalized;
        else
            avoid += Random.value < 0.5f ? leftDir.normalized : rightDir.normalized;

        avoid.y = 0f;

        if (avoid.sqrMagnitude <= 0.0001f)
            return Vector3.zero;

        return avoid.normalized * avoidanceStrength;
    }

    private bool CastObstacle(Vector3 origin, Vector3 dir, out RaycastHit hit, QueryTriggerInteraction triggerMode)
    {
        if (Physics.SphereCast(origin, avoidanceRadius, dir, out hit, avoidanceCheckDistance, obstacleMask, triggerMode))
        {
            if (IsOwnCollider(hit.collider))
                return false;

            return true;
        }

        return false;
    }

    private void UpdateStuckState(Vector3 desiredDir)
    {
        Vector3 flatVel = rb.linearVelocity;
        flatVel.y = 0f;

        bool tryingToMove = desiredDir.sqrMagnitude > 0.01f;
        bool barelyMoving = flatVel.magnitude < stuckSpeedThreshold;

        if (tryingToMove && barelyMoving)
        {
            stuckTimer += Time.fixedDeltaTime;

            if (stuckTimer >= stuckTimeThreshold && escapeTimer <= 0f)
            {
                BeginEscape(desiredDir);
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
    }

    private void BeginEscape(Vector3 desiredDir)
    {
        Vector3 baseDir = desiredDir.sqrMagnitude > 0.001f ? desiredDir.normalized : Random.insideUnitSphere;
        baseDir.y = 0f;

        Vector3 left = Quaternion.AngleAxis(-70f, Vector3.up) * baseDir;
        Vector3 right = Quaternion.AngleAxis(70f, Vector3.up) * baseDir;

        Vector3 origin = transform.position + Vector3.up * 0.1f;
        QueryTriggerInteraction triggerMode = ignoreTriggers
            ? QueryTriggerInteraction.Ignore
            : QueryTriggerInteraction.Collide;

        float leftScore = ScoreDirection(origin, left, triggerMode);
        float rightScore = ScoreDirection(origin, right, triggerMode);

        escapeDirection = leftScore > rightScore ? left.normalized : right.normalized;
        escapeTimer = escapeDuration;

        // Also repick target so it doesn't keep trying to force the same blocked path
        targetPoint = GetCenterBiasedPoint();
    }

    private float ScoreDirection(Vector3 origin, Vector3 dir, QueryTriggerInteraction triggerMode)
    {
        if (Physics.SphereCast(origin, avoidanceRadius, dir, out RaycastHit hit, avoidanceCheckDistance, obstacleMask, triggerMode))
        {
            if (IsOwnCollider(hit.collider))
                return 0f;

            return hit.distance;
        }

        return avoidanceCheckDistance;
    }

    private bool IsOwnCollider(Collider col)
    {
        for (int i = 0; i < ownColliders.Length; i++)
        {
            if (ownColliders[i] == col)
                return true;
        }
        return false;
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

    private void UpdateSpriteFlip()
    {
        if (spriteRenderer == null)
            return;

        float xVel = rb.linearVelocity.x;

        if (Mathf.Abs(xVel) < flipThreshold)
            return;

        bool movingRight = xVel > 0f;

        if (faceRightByDefault)
            spriteRenderer.flipX = !movingRight;
        else
            spriteRenderer.flipX = movingRight;
    }

    private void OnDrawGizmosSelected()
    {
        if (fishSpawnCenter != null)
            arenaCenter = fishSpawnCenter.transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(arenaCenter, new Vector3(arenaSize.x, 0.1f, arenaSize.y));

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetPoint, 0.2f);

        Vector3 baseDir = Application.isPlaying && rb != null && rb.linearVelocity.sqrMagnitude > 0.05f
            ? rb.linearVelocity.normalized
            : Vector3.forward;

        baseDir.y = 0f;

        Vector3 leftDir = Quaternion.AngleAxis(-sideFeelerAngle, Vector3.up) * baseDir;
        Vector3 rightDir = Quaternion.AngleAxis(sideFeelerAngle, Vector3.up) * baseDir;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + baseDir * avoidanceCheckDistance);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + leftDir * avoidanceCheckDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * avoidanceCheckDistance);
    }

}
