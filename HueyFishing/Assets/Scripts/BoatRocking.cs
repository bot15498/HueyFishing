using UnityEngine;
using DG.Tweening;

public class BoatRocking : MonoBehaviour
{
    [Header("References")]
    public Transform boatVisual;
    public Rigidbody rb;

    [Header("Rocking")]
    public float rockDuration = 2f;
    public float maxPitchIdle = 4f;
    public float maxRollIdle = 7f;
    public float maxPitchMoving = 1.5f;
    public float maxRollMoving = 2.5f;

    [Header("Speed Reference")]
    [Tooltip("Used to normalize speed into 0-1 for rocking blend.")]
    public float maxSpeed = 10f;

    [Tooltip("If true, uses rigidbody horizontal speed. If false, uses manualCurrentSpeed.")]
    public bool useRigidbodySpeed = true;

    [Tooltip("Only used if useRigidbodySpeed is false.")]
    public float manualCurrentSpeed = 0f;

    Tween rockTween;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        StartRocking();
    }

    void Update()
    {
        UpdateRocking();
    }

    void StartRocking()
    {
        if (boatVisual == null)
            return;

        rockTween?.Kill();

        boatVisual.localRotation = Quaternion.identity;

        rockTween = DOTween.To(
            () => 0f,
            value =>
            {
                float speedPercent = GetSpeedPercent();

                float pitchAmount = Mathf.Lerp(maxPitchIdle, maxPitchMoving, speedPercent);
                float rollAmount = Mathf.Lerp(maxRollIdle, maxRollMoving, speedPercent);

                float pitch = Mathf.Sin(value) * pitchAmount;
                float roll = Mathf.Cos(value * 0.85f) * rollAmount;

                boatVisual.localRotation = Quaternion.Euler(pitch, 0f, roll);
            },
            Mathf.PI * 2f,
            rockDuration
        )
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Restart);
    }

    void UpdateRocking()
    {
        if (rockTween == null || !rockTween.IsActive())
            return;

        float speedPercent = GetSpeedPercent();
        rockTween.timeScale = Mathf.Lerp(1f, 1.35f, speedPercent);
    }

    float GetSpeedPercent()
    {
        float currentSpeed = 0f;

        if (useRigidbodySpeed && rb != null)
        {
            Vector3 flatVelocity = rb.linearVelocity;
            flatVelocity.y = 0f;
            currentSpeed = flatVelocity.magnitude;
        }
        else
        {
            currentSpeed = Mathf.Abs(manualCurrentSpeed);
        }

        if (maxSpeed <= 0f)
            return 0f;

        return Mathf.Clamp01(currentSpeed / maxSpeed);
    }

    private void OnDisable()
    {
        rockTween?.Kill();
    }
}