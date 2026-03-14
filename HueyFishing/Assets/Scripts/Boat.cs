using Unity.VisualScripting;
using UnityEngine;

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


    private void Start()
    {
        canMove = true;
    }
    void Update()
    {

        if (canMove == true)
        {
            HandleMovement();
            HandleTurning();
        }
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
            currentSpeed += acceleration * Time.deltaTime;
        }
        else if (input < 0f)
        {
            currentSpeed -= reverseAcceleration * Time.deltaTime;
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, waterDrag * Time.deltaTime);
        }

        currentSpeed = Mathf.Clamp(currentSpeed, maxReverseSpeed, maxSpeed);

        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
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
            currentTurnSpeed += turnInput * turnAcceleration * Time.deltaTime;
        }
        else
        {
            currentTurnSpeed = Mathf.MoveTowards(currentTurnSpeed, 0f, turnDrag * Time.deltaTime);
        }

        currentTurnSpeed = Mathf.Clamp(currentTurnSpeed, -maxTurnSpeed, maxTurnSpeed);

        float speedFactor = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);
        speedFactor = Mathf.Max(speedFactor, minTurnFactor);

        float reverseFlip = currentSpeed < 0f ? -1f : 1f;

        transform.Rotate(Vector3.up * currentTurnSpeed * speedFactor * reverseFlip * Time.deltaTime);
    }

    void toggleCanmove()
    {
        canMove = !canMove;
    }

}




