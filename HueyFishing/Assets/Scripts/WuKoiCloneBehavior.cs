using UnityEngine;

public class WukoiCloneBehavior : FishMovement
{
    public enum CloneQuadrant
    {
        TopRight = 0,
        TopLeft = 1,
        BottomLeft = 2,
        BottomRight = 3
    }

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public bool faceRightByDefault = true;
    public float flipThreshold = 0.05f;

    [Header("Arena")]
    public Vector3 arenaCenter;
    public Vector2 arenaSize = new Vector2(8f, 5f);
    public CloneQuadrant assignedQuadrant = CloneQuadrant.TopRight;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float acceleration = 8f;
    public float arriveDistance = 0.4f;

    [Header("Wander Timing")]
    public float minPauseTime = 0.2f;
    public float maxPauseTime = 0.8f;
    public float minMoveTime = 0.7f;
    public float maxMoveTime = 1.6f;

    [Header("Curved Motion")]
    public float curveStrength = 0.35f;
    public float curveChangeInterval = 0.6f;

    [Header("Clone Separation")]
    [Tooltip("How far away clones try to keep from each other.")]
    public float separationRadius = 2.5f;

    [Tooltip("How strongly clones push away from each other.")]
    public float separationStrength = 5f;

    [Tooltip("Optional layer mask to limit what counts as another clone.")]
    public LayerMask cloneMask = ~0;

    [Header("Collider Avoidance")]
    public float avoidanceCheckDistance = 1.5f;
    public float avoidanceRadius = 0.35f;
    public float avoidanceStrength = 4f;

    [Range(5f, 80f)]
    public float sideFeelerAngle = 35f;

    public LayerMask obstacleMask = ~0;
    public bool ignoreTriggers = true;

    [Header("Stuck Recovery")]
    public float stuckSpeedThreshold = 0.15f;
    public float stuckTimeThreshold = 0.4f;
    public float escapeDuration = 0.5f;
    public float escapeStrength = 6f;

    [Header("Quadrant Padding")]
    [Tooltip("Keeps clones from hugging the exact borders of their quadrant.")]
    public float quadrantPadding = 0.5f;

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
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        ownColliders = GetComponentsInChildren<Collider>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (fishSpawnCenter != null)
            arenaCenter = fishSpawnCenter.transform.position;

        arenaCenter.y = 0f;
        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);

        if (rb != null)
        {
            rb.position = new Vector3(rb.position.x, 0f, rb.position.z);
            rb.linearVelocity = Vector3.zero;
        }

        PickNewPause();
        ForceFlatY();
    }

    public void SetQuadrant(CloneQuadrant quadrant)
    {
        assignedQuadrant = quadrant;
    }

    private void FixedUpdate()
    {
        if (fishSpawnCenter != null)
            arenaCenter = fishSpawnCenter.transform.position;

        arenaCenter.y = 0f;
        stateTimer -= Time.fixedDeltaTime;

        if (isPausing)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, acceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (stateTimer <= 0f)
                PickNewTarget();

            ClampInsideAssignedQuadrant();
            ForceFlatY();
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
            ClampInsideAssignedQuadrant();
            ForceFlatY();
            UpdateSpriteFlip();
            return;
        }

        Vector3 desiredDir = toTarget.normalized;

        Vector3 side = Vector3.Cross(Vector3.up, desiredDir);
        desiredDir += side * currentCurveOffset;

        desiredDir += GetSeparationForce();
        desiredDir += GetColliderAvoidance(desiredDir);
        desiredDir += GetQuadrantContainmentForce();

        UpdateStuckState(desiredDir);
        if (escapeTimer > 0f)
        {
            desiredDir += escapeDirection * escapeStrength;
            escapeTimer -= Time.fixedDeltaTime;
        }

        desiredDir.y = 0f;
        desiredDir = desiredDir.sqrMagnitude > 0.0001f ? desiredDir.normalized : Vector3.zero;

        Vector3 desiredVelocity = desiredDir * moveSpeed;
        desiredVelocity.y = 0f;

        rb.linearVelocity = Vector3.Lerp(
            rb.linearVelocity,
            desiredVelocity,
            acceleration * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        ClampInsideAssignedQuadrant();
        ForceFlatY();
        UpdateSpriteFlip();
    }

    private void PickNewTarget()
    {
        isPausing = false;
        stateTimer = Random.Range(minMoveTime, maxMoveTime);
        curveTimer = 0f;
        targetPoint = GetRandomPointInAssignedQuadrant();
        targetPoint.y = 0f;
    }

    private void PickNewPause()
    {
        isPausing = true;
        stateTimer = Random.Range(minPauseTime, maxPauseTime);
        currentCurveOffset = 0f;
        stuckTimer = 0f;
        escapeTimer = 0f;
    }

    private Vector3 GetRandomPointInAssignedQuadrant()
    {
        GetQuadrantBounds(out float minX, out float maxX, out float minZ, out float maxZ);

        float x = Random.Range(minX, maxX);
        float z = Random.Range(minZ, maxZ);

        return new Vector3(x, 0f, z);
    }

    private void GetQuadrantBounds(out float minX, out float maxX, out float minZ, out float maxZ)
    {
        float halfX = arenaSize.x * 0.5f;
        float halfZ = arenaSize.y * 0.5f;

        float arenaMinX = arenaCenter.x - halfX;
        float arenaMaxX = arenaCenter.x + halfX;
        float arenaMinZ = arenaCenter.z - halfZ;
        float arenaMaxZ = arenaCenter.z + halfZ;

        switch (assignedQuadrant)
        {
            case CloneQuadrant.TopRight:
                minX = arenaCenter.x + quadrantPadding;
                maxX = arenaMaxX - quadrantPadding;
                minZ = arenaCenter.z + quadrantPadding;
                maxZ = arenaMaxZ - quadrantPadding;
                break;

            case CloneQuadrant.TopLeft:
                minX = arenaMinX + quadrantPadding;
                maxX = arenaCenter.x - quadrantPadding;
                minZ = arenaCenter.z + quadrantPadding;
                maxZ = arenaMaxZ - quadrantPadding;
                break;

            case CloneQuadrant.BottomLeft:
                minX = arenaMinX + quadrantPadding;
                maxX = arenaCenter.x - quadrantPadding;
                minZ = arenaMinZ + quadrantPadding;
                maxZ = arenaCenter.z - quadrantPadding;
                break;

            default: // BottomRight
                minX = arenaCenter.x + quadrantPadding;
                maxX = arenaMaxX - quadrantPadding;
                minZ = arenaMinZ + quadrantPadding;
                maxZ = arenaCenter.z - quadrantPadding;
                break;
        }

        if (minX > maxX)
        {
            float mid = (minX + maxX) * 0.5f;
            minX = mid;
            maxX = mid;
        }

        if (minZ > maxZ)
        {
            float mid = (minZ + maxZ) * 0.5f;
            minZ = mid;
            maxZ = mid;
        }
    }

    private Vector3 GetQuadrantContainmentForce()
    {
        GetQuadrantBounds(out float minX, out float maxX, out float minZ, out float maxZ);

        Vector3 pos = transform.position;
        Vector3 push = Vector3.zero;
        float buffer = 0.75f;

        if (pos.x < minX + buffer)
            push += Vector3.right * ((minX + buffer - pos.x) / buffer);
        else if (pos.x > maxX - buffer)
            push += Vector3.left * ((pos.x - (maxX - buffer)) / buffer);

        if (pos.z < minZ + buffer)
            push += Vector3.forward * ((minZ + buffer - pos.z) / buffer);
        else if (pos.z > maxZ - buffer)
            push += Vector3.back * ((pos.z - (maxZ - buffer)) / buffer);

        return push * 3f;
    }

    private Vector3 GetSeparationForce()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            separationRadius,
            cloneMask,
            ignoreTriggers ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide
        );

        Vector3 force = Vector3.zero;
        int count = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider col = hits[i];

            if (IsOwnCollider(col))
                continue;

            WukoiCloneBehavior otherClone = col.GetComponentInParent<WukoiCloneBehavior>();
            if (otherClone == null || otherClone == this)
                continue;

            Vector3 away = transform.position - otherClone.transform.position;
            away.y = 0f;

            float dist = away.magnitude;
            if (dist <= 0.001f)
                continue;

            float strength = 1f - Mathf.Clamp01(dist / separationRadius);
            force += away.normalized * strength;
            count++;
        }

        if (count == 0 || force.sqrMagnitude <= 0.0001f)
            return Vector3.zero;

        return force.normalized * separationStrength;
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

        if (!hitForward && !hitLeft && !hitRight)
            return Vector3.zero;

        Vector3 avoid = Vector3.zero;

        if (hitForward)
        {
            Vector3 normalPush = forwardHit.normal;
            normalPush.y = 0f;

            float closeness = 1f - (forwardHit.distance / avoidanceCheckDistance);
            avoid += normalPush.normalized * closeness;
        }

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

        if (baseDir.sqrMagnitude <= 0.0001f)
            baseDir = Vector3.forward;

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
        targetPoint = GetRandomPointInAssignedQuadrant();
        targetPoint.y = 0f;
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

    private void ClampInsideAssignedQuadrant()
    {
        GetQuadrantBounds(out float minX, out float maxX, out float minZ, out float maxZ);

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        pos.y = 0f;

        rb.MovePosition(pos);
    }

    private void ForceFlatY()
    {
        Vector3 pos = rb.position;
        pos.y = 0f;
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
        arenaCenter.y = 0f;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(arenaCenter, new Vector3(arenaSize.x, 0.1f, arenaSize.y));

        GetQuadrantBounds(out float minX, out float maxX, out float minZ, out float maxZ);
        Vector3 quadCenter = new Vector3((minX + maxX) * 0.5f, 0f, (minZ + maxZ) * 0.5f);
        Vector3 quadSize = new Vector3(maxX - minX, 0.1f, maxZ - minZ);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(quadCenter, quadSize);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetPoint, 0.2f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}