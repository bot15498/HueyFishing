using UnityEngine;

public class StunBubble : MonoBehaviour
{
    public bool isLockedOnObject = false;
    public Transform targetTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isLockedOnObject && targetTransform != null)
        {
            transform.position = targetTransform.position;
        }
        else
        {
            // Keep moving forward
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check to see if it's an enemy
        // if it's not an enemy, then pop and kill self


        // Set the stun flag on them
        // change the 
    }
}
