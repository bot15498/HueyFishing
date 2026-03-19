using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
public class Boat : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 5f;
    public float reverseAcceleration = 3f;
    public float maxSpeed = 10f;
    public float maxReverseSpeed = -4f;
    public float waterDrag = 2f;

    [Header("Turning")]
    public float turnAcceleration = 80f;
    public float maxTurnSpeed = 90f;
    public float turnDrag = 120f;

    [Range(0f, 1f)] public float minTurnFactor = 0.2f;

    float currentSpeed = 0f;
    float currentTurnSpeed = 0f;

    public bool canMove;

    Rigidbody rb;

    [Header("Boat Rock")]
    public Transform boatVisual;

    public float rockSpeed = 2f;
    public float rockAmountMoving = 2f;
    public float rockAmountIdle = 8f;

    Tween rockTween;


    [Header("Rocking")]
    public float rockDuration = 2f;
    public float maxPitchIdle = 4f;   // forward/back
    public float maxRollIdle = 7f;    // side to side
    public float maxPitchMoving = 1.5f;
    public float maxRollMoving = 2.5f;

    [Header("Turn Visual")]
    public Transform turnVisual;
    public Transform turnVisual2;
    public float maxVisualYaw = 30f;
    public float visualTurnTweenTime = 0.15f;

    Tween turnVisualTween;
    float lastVisualYaw;



    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        canMove = true;
        StartRocking();
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            HandleMovement();
            HandleTurning();

        }
        UpdateRocking();
        UpdateTurnVisual();
    }

    void HandleMovement()
    {
        float input = 0f;

        if (Input.GetKey(KeyCode.W))
            input = 1f;
        else if (Input.GetKey(KeyCode.S))
            input = -1f;

        if (input > 0f)
        {
            currentSpeed += acceleration * Time.fixedDeltaTime;
        }
        else if (input < 0f)
        {
            currentSpeed -= reverseAcceleration * Time.fixedDeltaTime;
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, waterDrag * Time.fixedDeltaTime);
        }

        currentSpeed = Mathf.Clamp(currentSpeed, maxReverseSpeed, maxSpeed);

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        rb.linearVelocity = new Vector3(
            forward.x * currentSpeed,
            rb.linearVelocity.y,
            forward.z * currentSpeed);
    }

    void HandleTurning()
    {
        float turnInput = 0f;

        if (Input.GetKey(KeyCode.A))
            turnInput = -1f;
        else if (Input.GetKey(KeyCode.D))
            turnInput = 1f;

        if (turnInput != 0f)
        {
            currentTurnSpeed += turnInput * turnAcceleration * Time.fixedDeltaTime;
        }
        else
        {
            currentTurnSpeed = Mathf.MoveTowards(currentTurnSpeed, 0f, turnDrag * Time.fixedDeltaTime);
        }

        currentTurnSpeed = Mathf.Clamp(currentTurnSpeed, -maxTurnSpeed, maxTurnSpeed);

        float speedFactor = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);
        speedFactor = Mathf.Max(speedFactor, minTurnFactor);

        float reverseFlip = currentSpeed < 0f ? -1f : 1f;

        float turnAmount = currentTurnSpeed * speedFactor * reverseFlip * Time.fixedDeltaTime;

        Quaternion rotation = Quaternion.Euler(0f, turnAmount, 0f);

        rb.MoveRotation(rb.rotation * rotation);
    }

    public void toggleCanmove()
    {
        canMove = !canMove;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void StartRocking()
    {
        if (boatVisual == null)
            return;

        rockTween?.Kill();

        Vector3 startRot = boatVisual.localEulerAngles;
        boatVisual.localRotation = Quaternion.Euler(0f, startRot.y, 0f);

        rockTween = DOTween.To(
            () => 0f,
            value =>
            {
                float speedPercent = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);

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

        
        float speedPercent = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);
        rockTween.timeScale = Mathf.Lerp(1f, 1.35f, speedPercent);

    }


    private void OnDisable()
    {
        rockTween?.Kill();

    }

    void UpdateTurnVisual()
    {
        if (turnVisual == null)
            return;

        float normalizedTurn = 0f;

        if (maxTurnSpeed > 0f)
            normalizedTurn = Mathf.Clamp(currentTurnSpeed / maxTurnSpeed, -1f, 1f);

        float targetYaw = normalizedTurn * maxVisualYaw;

        if (Mathf.Abs(targetYaw - lastVisualYaw) < 0.05f)
            return;

        lastVisualYaw = targetYaw;

        if (turnVisualTween != null && turnVisualTween.IsActive())
            turnVisualTween.Kill();

        Vector3 currentRot = turnVisual.localEulerAngles;
        float currentX = turnVisual.localEulerAngles.x;
        float currentZ = turnVisual.localEulerAngles.z;

        turnVisualTween = turnVisual
            .DOLocalRotate(new Vector3(
                NormalizeAngle(currentX),
                targetYaw,
                NormalizeAngle(currentZ)
            ), visualTurnTweenTime)
            .SetEase(Ease.OutSine);




        turnVisualTween = turnVisual
            .DOLocalRotate(new Vector3(
                NormalizeAngle(currentX),
                targetYaw,
                NormalizeAngle(currentZ)
            ), visualTurnTweenTime)
            .SetEase(Ease.OutSine);



    }

    float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}




