using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

public class DrawingManager : MonoBehaviour
{
    public Camera currCamera;
    public float maxSegmentLength = 1f;
    public float segmentWidth = 1f;
    public float drawDelayAfterBreak = 0.5f;
    [SerializeField]
    private Material lineMaterial;
    [SerializeField]
    private Vector3 startingPoint = Vector3.zero;
    [SerializeField]
    private List<CatchTrailCollider> segments = new List<CatchTrailCollider>();
    [SerializeField]
    private CatchTrailCollider lastSegment = null;
    private GameObject fishingLine = null;
    private bool clearSegmentsOnNextCycle = false;
    private FishManager fishManager;
    private PlayerHealthManager playerHealthManager;
    private float drawDelayCurrTime = 0f;

    void Start()
    {
        startingPoint = Vector3.zero;
        segments = new List<CatchTrailCollider>();
        fishManager = GetComponent<FishManager>();
        playerHealthManager = GetComponent<PlayerHealthManager>();
    }

    void Update()
    {
        // Methodology:
        // When left mouse is clicked, track where the cursor is on the floor. Save the initial point and create a trigger 
        // As cursor moves, change size of trigger to draw betwen the original point and the cursor.
        // When left click is released, delete all triggers
        if (Input.GetMouseButton(0) && drawDelayCurrTime <= 0)
        {
            // Start drawing
            DrawSegments();

            // Do a check to see if we shouldn't be able to be drawing right now
            if (clearSegmentsOnNextCycle)
            {
                DeleteAllSegments();
                clearSegmentsOnNextCycle = false;
            }
        }
        else
        {
            // Release mouse click, clear everything
            DeleteAllSegments();
        }

        if (drawDelayCurrTime > 0)
        {
            drawDelayCurrTime -= Time.deltaTime;
        }
    }

    public void TriggerCatchCircleComplete(int firstid, int lastid)
    {
        // Called when triggers detect that there is circle closure.
        // Get the list of segments that  we care about
        var segmentPoints = new List<Vector3>();
        foreach (var segs in segments)
        {
            if (segs.id > lastid)
            {
                // Done
                break;
            }
            if (segs.id >= firstid || segs.id <= lastid)
            {
                segmentPoints.Add(segs.startpoint);
                segmentPoints.Add(segs.endpoint);
            }
        }

        // Check for every fish in the scene if it's inside the region
        foreach (var fish in fishManager.currentFish)
        {
            if (fish != null && IsPointInPolygon(fish.transform.position, segmentPoints))
            {
                fish.IncreaseCatchBar(playerHealthManager.circlePower);
            }
        }

        // Stage to delete segments since we are done with them now
        clearSegmentsOnNextCycle = true;
    }

    public void TriggerLineBreak()
    {
        if(!playerHealthManager.isLineUnbreakable)
        {
            // Makes it so the line is broken and the player has to restart
            // Also puts a delay before the player can start drawing a line again
            clearSegmentsOnNextCycle = true;

            // Also make it so there is a small delay before you can draw again to prevent
            // players from breaking the line again
            drawDelayCurrTime = drawDelayAfterBreak;
        }
    }

    public void DoDamageToPlayer(int damage)
    {
        playerHealthManager.DoDamageToPlayer(damage);
    }

    public void BuildPlayerAbilityGuage(int amount = 10)
    {
        playerHealthManager.AddPlayerAbilityGuage(amount);
    }

    private void DrawSegments()
    {
        Ray ray = currCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("CatchingFloor")))
        {
            //Debug.Log(hit.point);
            if (startingPoint == Vector3.zero)
            {
                // Starting case. Create an object to hold the line renderer
                CreateLineRenderer();

                // Now draw the line
                startingPoint = hit.point;
                CreateSegmentObject(hit.point);
            }
            else if ((startingPoint - hit.point).sqrMagnitude >= maxSegmentLength * maxSegmentLength)
            {
                // We've hit max segment length, so snap a new trigger
                // Update the most recent trigger one last time
                UpdateSegmentSize(lastSegment, startingPoint, hit.point);

                // Create new trigger
                startingPoint = hit.point;
                CreateSegmentObject(hit.point);
            }
            else
            {
                // Update the parameters of the current trigger
                UpdateSegmentSize(lastSegment, startingPoint, hit.point);

                // Update the line renderer
                UpdateLineRenderer();
            }
        }
    }

    private void CreateSegmentObject(Vector3 origin)
    {
        GameObject go = new GameObject("Trail Segment");
        go.tag = CatchTrailCollider.ColliderTag;
        go.transform.position = origin;

        BoxCollider collider = go.AddComponent<BoxCollider>();
        collider.enabled = true;
        collider.isTrigger = true;

        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.useGravity = false;

        CatchTrailCollider ctc = go.AddComponent<CatchTrailCollider>();
        ctc.drawingManager = this;
        ctc.timeoutTime = playerHealthManager.segmentTimeoutTime;
        if (segments.Count > 0)
        {
            ctc.id = segments.Last().id + 1;
        }

        // Add to list of segments
        segments.Add(ctc);
        lastSegment = ctc;

        // Check for max number of segments
        MaxSegmentCheck();
    }

    private void UpdateSegmentSize(CatchTrailCollider segs, Vector3 startpoint, Vector3 endpoint)
    {
        // Segment size is based on z
        segs.transform.position = (startpoint + endpoint) / 2f;
        segs.transform.LookAt(endpoint);
        BoxCollider segsTrigger = segs.GetComponent<BoxCollider>();
        segsTrigger.size = new Vector3(segmentWidth, segmentWidth, Vector3.Distance(startpoint, endpoint));

        segs.startpoint = startpoint;
        segs.endpoint = endpoint;
    }

    private void MaxSegmentCheck()
    {
        if (segments.Count > playerHealthManager.maxLineLength)
        {
            // Delete the head of the list
            GameObject todelete = segments[0].gameObject;
            segments.RemoveAt(0);
            Destroy(todelete);
        }
    }

    private void DeleteAllSegments()
    {
        foreach (var segs in segments)
        {
            Destroy(segs.gameObject);
        }
        segments.Clear();
        lastSegment = null;
        DeleteLineRenderer();

        startingPoint = Vector3.zero;
    }

    public void DeleteSegment(CatchTrailCollider segsToDelete)
    {
        if(segments.Count > 1)
        {
            segments.Remove(segsToDelete);
            Destroy(segsToDelete.gameObject);
        }
    }

    private bool IsPointInPolygon(Vector3 target, List<Vector3> polygonPoints)
    {
        // Implementation of the rayscast algorithm for checking if a point is 
        // inside a polygon.
        // I understand the concept but have no idea what exactly this code is doing
        // https://alienryderflex.com/polygon/
        bool inside = false;

        for (int i = 0, j = polygonPoints.Count - 1; i < polygonPoints.Count; j = i++)
        {
            if (((polygonPoints[i].z > target.z) != (polygonPoints[j].z > target.z)) &&
                (target.x < (polygonPoints[j].x - polygonPoints[i].x) *
                 (target.z - polygonPoints[i].z) /
                 (polygonPoints[j].z - polygonPoints[i].z) + polygonPoints[i].x))
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private void CreateLineRenderer()
    {
        fishingLine = new GameObject("Fishing Line");
        var renderer = fishingLine.AddComponent<LineRenderer>();
        renderer.startWidth = segmentWidth;
        renderer.material = lineMaterial;
        //renderer.numCornerVertices = 3;
    }

    private void DeleteLineRenderer()
    {
        Destroy(fishingLine);
    }

    private void UpdateLineRenderer()
    {
        if (fishingLine != null)
        {
            LineRenderer renderer = fishingLine.GetComponent<LineRenderer>();
            renderer.positionCount = segments.Count * 2;
            int rendererPos = 0;
            foreach (var segs in segments)
            {
                renderer.SetPosition(rendererPos, segs.startpoint + Vector3.up * segmentWidth * 0.5f);
                renderer.SetPosition(rendererPos + 1, segs.endpoint + Vector3.up * segmentWidth * 0.5f);
                rendererPos += 2;
            }
        }
    }
}
