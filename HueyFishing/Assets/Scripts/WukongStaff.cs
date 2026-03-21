using UnityEngine;

public class WukongStaff : MonoBehaviour
{
    [Header("Spin")]
    public float spinSpeed = 180f;
    public Vector3 spinAxis = Vector3.up;

    [Header("Scale (Multipliers)")]
    public float scaleMultiplierA = 1f;
    public float scaleMultiplierB = 1.2f;
    public float scaleSpeed = 1f;

    [Header("Collision")]
    [Tooltip("Ignore collisions with parent colliders.")]
    public bool ignoreParentColliders = true;

    [Tooltip("Ignore collisions with all ancestors (not just direct parent).")]
    public bool ignoreAllParentHierarchy = true;

    [Tooltip("Also ignore trigger colliders.")]
    public bool includeTriggers = true;

    public bool useUnscaledTime = false;

    private float scaleT;
    private Vector3 baseScale;

    private void Start()
    {
        baseScale = transform.localScale;

        if (ignoreParentColliders)
            IgnoreParentCollisions();
    }

    private void Update()
    {
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        UpdateSpin(dt);
        UpdateScale(dt);
    }

    private void UpdateSpin(float dt)
    {
        Vector3 axis = spinAxis.sqrMagnitude > 0.0001f ? spinAxis.normalized : Vector3.up;
        transform.Rotate(axis, spinSpeed * dt, Space.Self);
    }

    private void UpdateScale(float dt)
    {
        scaleT += scaleSpeed * dt;

        float lerp = Mathf.PingPong(scaleT, 1f);
        lerp = Mathf.SmoothStep(0f, 1f, lerp);

        float currentMultiplier = Mathf.Lerp(scaleMultiplierA, scaleMultiplierB, lerp);
        transform.localScale = baseScale * currentMultiplier;
    }

    private void IgnoreParentCollisions()
    {
        Collider[] myColliders = GetComponentsInChildren<Collider>();

        if (myColliders.Length == 0)
            return;

        Transform current = transform.parent;

        while (current != null)
        {
            Collider[] parentColliders = current.GetComponentsInChildren<Collider>();

            foreach (var myCol in myColliders)
            {
                foreach (var parentCol in parentColliders)
                {
                    if (!includeTriggers && (myCol.isTrigger || parentCol.isTrigger))
                        continue;

                    Physics.IgnoreCollision(myCol, parentCol, true);
                }
            }

            if (!ignoreAllParentHierarchy)
                break;

            current = current.parent;
        }
    }

    private void OnValidate()
    {
        if (scaleSpeed < 0f)
            scaleSpeed = 0f;
    }
}
