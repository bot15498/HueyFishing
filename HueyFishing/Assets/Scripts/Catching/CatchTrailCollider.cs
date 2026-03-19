using UnityEngine;

public class CatchTrailCollider : MonoBehaviour
{
    public static string ColliderTag = "CatchingTrail";
    public static string FloorTag = "CatchingFloor";

    public int id = 0;
    public float timeoutTime = 5f;
    public Vector3 startpoint = Vector3.zero;
    public Vector3 endpoint = Vector3.zero;
    public DrawingManager drawingManager;
    private float currTime = 0f;
    public bool isDoingCircleCheck = false;

    private void Update()
    {
        currTime += Time.deltaTime;
        if (currTime > timeoutTime)
        {
            // Time to die, alert the drawing manager
            currTime = 0;
            drawingManager.DeleteSegment(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger && other.tag == ColliderTag)
        {
            // Do a check to make sure we are at least two segments away from the last trail segment
            // Also make sure to fire the cross only on the larger id
            CatchTrailCollider otherCtc = other.gameObject.GetComponent<CatchTrailCollider>();
            if (!isDoingCircleCheck && !otherCtc.isDoingCircleCheck && otherCtc != null && Mathf.Abs(id - otherCtc.id) > 2 && id > otherCtc.id)
            {
                // Turn a flag on both the colliders, so they won't double count
                isDoingCircleCheck = true;
                otherCtc.isDoingCircleCheck = true;
                // Tell the drawing manager to delete on the next cycle.
                drawingManager.TriggerCatchCircleComplete(otherCtc.id, id);
            }
        }
        else if (other.tag != FloorTag)
        {
            // Hit something else, take damage
            var dl = other.gameObject.GetComponent<DamageLine>();
            if (dl != null)
            {
                drawingManager.DoDamageToPlayer(dl.playerDamageAmount);
            }

            // Now break the line
            drawingManager.TriggerLineBreak();
        }
    }

    private void OnDrawGizmos()
    {
        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider != null)
        {
            Gizmos.color = Color.greenYellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(collider.center, collider.size);
        }
    }
}
