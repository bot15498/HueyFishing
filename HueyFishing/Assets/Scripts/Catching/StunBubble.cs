using System.Collections;
using UnityEngine;

public class StunBubble : MonoBehaviour
{
    [SerializeField]
    private int damageAmount = 10;
    public float lifetime = 5f;
    public Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetVelocity(Vector3 vel)
    {
        if (rb != null)
        {
            rb.linearVelocity = vel;
        }
    }

    public void SetDamageAmount(int amount)
    {
        damageAmount = amount;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != CatchTrailCollider.FloorTag && other.tag != CatchTrailCollider.ColliderTag && other.tag != "Bubble")
        {
            // Hit something else, take damage
            var fc = other.gameObject.GetComponent<FishCatchbar>();
            if (fc != null)
            {
                fc.IncreaseCatchBar(damageAmount);
            }

            // Now kill self
            Destroy(gameObject);
        }
    }
}
