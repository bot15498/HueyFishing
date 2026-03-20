using UnityEngine;

public class EelProjectile : MonoBehaviour
{
    public float lifetime = 5f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(Vector3 velocity, Collider[] collidersToIgnore)
    {
        if (rb != null)
            rb.linearVelocity = velocity;

        Collider myCol = GetComponent<Collider>();
        if (myCol != null && collidersToIgnore != null)
        {
            for (int i = 0; i < collidersToIgnore.Length; i++)
            {
                if (collidersToIgnore[i] != null)
                    Physics.IgnoreCollision(myCol, collidersToIgnore[i], true);
            }
        }

        Destroy(gameObject, lifetime);
    }
}