using System.Collections.Generic;
using UnityEngine;

public class EelBehavior : FishMovement
{
    [Header("References")]
    public Transform head;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public bool faceRightByDefault = true;
    public float flipThreshold = 0.01f;

    [Header("Arena")]
    public Vector3 arenaCenter;
    public Vector2 arenaSize = new Vector2(16f, 10f);

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float turnSpeed = 5f;
    public float arriveDistance = 0.4f;

    [Header("Pause Timing")]
    public float minPauseTime = 0.3f;
    public float maxPauseTime = 0.8f;

    [Header("Cross Movement")]
    [Range(0.6f, 1f)]
    public float edgeTargetPercent = 0.9f;
    public bool moveAcrossX = true;

    [Header("Target Selection")]
    [Tooltip("Higher = more likely to pick target positions near the arena center on the non-crossing axis.")]
    public float centerFavorability = 2.5f;

    [Header("Curving")]
    public float curveStrength = 0.6f;
    public float curveFrequency = 1.2f;
    private float curveSeed;

    [Header("Segments")]
    public Transform[] bodySegments;
    public Transform[] tailSegments;

    public float bodySpacing = 0.35f;
    public float tailSpacing = 0.2f;

    public float bodySharpness = 14f;
    public float tailSharpness = 18f;
    public float lookSharpness = 12f;

    [Header("Trail")]
    public float trailSpacing = 0.06f;

    [Header("Projectile Attack")]
    [Tooltip("Projectile prefab with a Collider. Rigidbody is optional but recommended.")]
    public GameObject projectilePrefab;

    [Tooltip("Delay between shots.")]
    public float shotDelay = 1.2f;

    [Tooltip("Projectile speed.")]
    public float projectileSpeed = 8f;

    [Tooltip("Spawn slightly away from the segment so it doesn't overlap.")]
    public float projectileSpawnOffset = 0.15f;

    [Tooltip("If true, can fire while paused.")]
    public bool shootWhilePaused = true;

    [Tooltip("Optional custom fire points. If empty, random body/tail segments are used.")]
    public Transform[] projectileFireSegments;

    private readonly List<Vector3> trail = new List<Vector3>();
    private readonly List<Transform> cachedFireSegments = new List<Transform>();

    private Vector3 targetPoint;
    private bool isPausing;
    private float timer;
    private int side = 1;
    private float shotTimer;

    private Collider[] ownColliders;

    private void Start()
    {
        if (head == null)
        {
            Debug.LogError("EelBehavior: head is not assigned.", this);
            enabled = false;
            return;
        }

        if (fishSpawnCenter != null)
            arenaCenter = fishSpawnCenter.transform.position;

        curveSeed = Random.Range(0f, 100f);

        CacheOwnColliders();
        CacheFireSegments();

        ResetTrail();
        IgnoreSelfCollisions();

        head.position = Clamp(head.position);
        side = GetStartingSide();
        StartPause();

        shotTimer = shotDelay;
    }

    private void Update()
    {
        if (head == null)
            return;

        if (fishSpawnCenter != null)
            arenaCenter = fishSpawnCenter.transform.position;

        HandleProjectileFiring();

        timer -= Time.deltaTime;

        if (isPausing)
        {
            if (timer <= 0f)
                StartMove();

            UpdateTrail();
            UpdateSegments();
            return;
        }

        Vector3 toTarget = targetPoint - head.position;
        toTarget.y = 0f;

        if (toTarget.magnitude <= arriveDistance)
        {
            StartPause();
            return;
        }

        Vector3 baseDir = toTarget.normalized;

        Vector3 sideDir = Vector3.Cross(Vector3.up, baseDir);
        float curve = Mathf.Sin((Time.time + curveSeed) * curveFrequency) * curveStrength;

        Vector3 moveDir = (baseDir + sideDir * curve).normalized;

        head.position += moveDir * moveSpeed * Time.deltaTime;
        head.position = Clamp(head.position);

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            head.rotation = Quaternion.Slerp(head.rotation, targetRot, turnSpeed * Time.deltaTime);
        }

        UpdateTrail();
        UpdateSegments();
        UpdateSpriteFlip(moveDir);
    }

    private void HandleProjectileFiring()
    {
        if (projectilePrefab == null)
            return;

        if (!shootWhilePaused && isPausing)
            return;

        if (cachedFireSegments.Count == 0)
            return;

        shotTimer -= Time.deltaTime;
        if (shotTimer > 0f)
            return;

        FireProjectileFromRandomSegment();
        shotTimer = shotDelay;
    }

    private void FireProjectileFromRandomSegment()
    {
        Transform firePoint = GetRandomFireSegment();
        if (firePoint == null)
            return;

        Vector3 targetPos = transform.position;
        Vector3 dir = targetPos - firePoint.position;
        dir.y = 0f;

        if (dir.sqrMagnitude <= 0.0001f)
            dir = transform.forward.sqrMagnitude > 0.0001f ? transform.forward : Vector3.forward;

        dir.Normalize();

        Vector3 spawnPos = firePoint.position + dir * projectileSpawnOffset;
        Quaternion spawnRot = Quaternion.LookRotation(dir, Vector3.up);

        GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, spawnRot);

        Collider projectileCol = projectileObj.GetComponent<Collider>();
        if (projectileCol != null && ownColliders != null)
        {
            for (int i = 0; i < ownColliders.Length; i++)
            {
                if (ownColliders[i] != null)
                    Physics.IgnoreCollision(projectileCol, ownColliders[i], true);
            }
        }

        EelProjectile projectile = projectileObj.GetComponent<EelProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(dir * projectileSpeed, ownColliders);
        }
        else
        {
            Rigidbody rb = projectileObj.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = dir * projectileSpeed;
        }
    }

    private Transform GetRandomFireSegment()
    {
        if (cachedFireSegments.Count == 0)
            return null;

        for (int tries = 0; tries < 20; tries++)
        {
            Transform t = cachedFireSegments[Random.Range(0, cachedFireSegments.Count)];
            if (t != null)
                return t;
        }

        return null;
    }

    private void CacheOwnColliders()
    {
        ownColliders = GetComponentsInChildren<Collider>(true);
    }

    private void CacheFireSegments()
    {
        cachedFireSegments.Clear();

        if (projectileFireSegments != null && projectileFireSegments.Length > 0)
        {
            for (int i = 0; i < projectileFireSegments.Length; i++)
            {
                if (projectileFireSegments[i] != null)
                    cachedFireSegments.Add(projectileFireSegments[i]);
            }

            return;
        }

        if (bodySegments != null)
        {
            for (int i = 0; i < bodySegments.Length; i++)
            {
                if (bodySegments[i] != null)
                    cachedFireSegments.Add(bodySegments[i]);
            }
        }

        if (tailSegments != null)
        {
            for (int i = 0; i < tailSegments.Length; i++)
            {
                if (tailSegments[i] != null)
                    cachedFireSegments.Add(tailSegments[i]);
            }
        }
    }

    private void StartPause()
    {
        isPausing = true;
        timer = Random.Range(minPauseTime, maxPauseTime);
    }

    private void StartMove()
    {
        isPausing = false;
        side *= -1;
        targetPoint = Clamp(GetSideTarget(side));
    }

    private int GetStartingSide()
    {
        Vector3 local = head.position - arenaCenter;

        if (moveAcrossX)
            return local.x >= 0f ? 1 : -1;
        else
            return local.z >= 0f ? 1 : -1;
    }

    private Vector3 GetSideTarget(int s)
    {
        float halfX = arenaSize.x * 0.5f;
        float halfZ = arenaSize.y * 0.5f;

        float edgeX = halfX * edgeTargetPercent;
        float edgeZ = halfZ * edgeTargetPercent;

        if (moveAcrossX)
        {
            return new Vector3(
                arenaCenter.x + edgeX * s,
                head.position.y,
                arenaCenter.z + GetCenteredBiasedOffset(edgeZ)
            );
        }
        else
        {
            return new Vector3(
                arenaCenter.x + GetCenteredBiasedOffset(edgeX),
                head.position.y,
                arenaCenter.z + edgeZ * s
            );
        }
    }

    private float GetCenteredBiasedOffset(float halfExtent)
    {
        float sign = Random.value < 0.5f ? -1f : 1f;

        float t = Random.value;
        if (centerFavorability > 1f)
            t = Mathf.Pow(t, centerFavorability);

        return sign * t * halfExtent;
    }

    private Vector3 Clamp(Vector3 pos)
    {
        float halfX = arenaSize.x * 0.5f;
        float halfZ = arenaSize.y * 0.5f;

        pos.x = Mathf.Clamp(pos.x, arenaCenter.x - halfX, arenaCenter.x + halfX);
        pos.z = Mathf.Clamp(pos.z, arenaCenter.z - halfZ, arenaCenter.z + halfZ);
        pos.y = head.position.y;

        return pos;
    }

    private void ResetTrail()
    {
        trail.Clear();

        for (int i = 0; i < 100; i++)
            trail.Add(head.position);
    }

    private void UpdateTrail()
    {
        if (trail.Count == 0)
        {
            trail.Add(head.position);
            return;
        }

        float dist = Vector3.Distance(trail[0], head.position);

        if (dist >= trailSpacing)
            trail.Insert(0, head.position);
        else
            trail[0] = head.position;

        if (trail.Count > 200)
            trail.RemoveAt(trail.Count - 1);
    }

    private void UpdateSegments()
    {
        UpdateChain(bodySegments, bodySpacing, bodySharpness, 0f);

        float bodyLen = (bodySegments != null ? bodySegments.Length : 0) * bodySpacing;
        UpdateChain(tailSegments, tailSpacing, tailSharpness, bodyLen);
    }

    private void UpdateChain(Transform[] segments, float spacing, float sharpness, float offset)
    {
        if (segments == null) return;

        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i] == null)
                continue;

            float dist = offset + spacing * (i + 1);
            Vector3 target = GetTrailPoint(dist);

            segments[i].position = Vector3.Lerp(
                segments[i].position,
                target,
                sharpness * Time.deltaTime
            );

            Vector3 lookDir = (i == 0)
                ? head.position - segments[i].position
                : segments[i - 1].position - segments[i].position;

            lookDir.y = 0f;

            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion rot = Quaternion.LookRotation(lookDir, Vector3.up);
                segments[i].rotation = Quaternion.Slerp(
                    segments[i].rotation,
                    rot,
                    lookSharpness * Time.deltaTime
                );
            }
        }
    }

    private Vector3 GetTrailPoint(float distance)
    {
        float remaining = distance;

        for (int i = 0; i < trail.Count - 1; i++)
        {
            float d = Vector3.Distance(trail[i], trail[i + 1]);

            if (remaining <= d)
                return Vector3.Lerp(trail[i], trail[i + 1], remaining / d);

            remaining -= d;
        }

        return trail[trail.Count - 1];
    }

    private void IgnoreSelfCollisions()
    {
        Collider[] cols = GetComponentsInChildren<Collider>();

        for (int i = 0; i < cols.Length; i++)
        {
            if (cols[i] == null) continue;

            for (int j = i + 1; j < cols.Length; j++)
            {
                if (cols[j] == null) continue;
                Physics.IgnoreCollision(cols[i], cols[j], true);
            }
        }
    }

    private void UpdateSpriteFlip(Vector3 dir)
    {
        if (!spriteRenderer) return;

        if (Mathf.Abs(dir.x) < flipThreshold) return;

        bool right = dir.x > 0f;
        spriteRenderer.flipX = faceRightByDefault ? !right : right;
    }

    private void OnDrawGizmosSelected()
    {
        if (head == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(arenaCenter, new Vector3(arenaSize.x, 0.1f, arenaSize.y));

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetPoint, 0.2f);

        Gizmos.color = Color.white;
        for (int i = 0; i < trail.Count - 1; i += 5)
        {
            Gizmos.DrawLine(trail[i], trail[i + 1]);
        }

        Gizmos.color = Color.green;
        if (bodySegments != null)
        {
            foreach (var seg in bodySegments)
                if (seg) Gizmos.DrawSphere(seg.position, 0.07f);
        }

        Gizmos.color = Color.blue;
        if (tailSegments != null)
        {
            foreach (var seg in tailSegments)
                if (seg) Gizmos.DrawSphere(seg.position, 0.05f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(head.position, 0.1f);

        if (cachedFireSegments != null)
        {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < cachedFireSegments.Count; i++)
            {
                if (cachedFireSegments[i] != null)
                    Gizmos.DrawSphere(cachedFireSegments[i].position, 0.08f);
            }
        }
    }
}