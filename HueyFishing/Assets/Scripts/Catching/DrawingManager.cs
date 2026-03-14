using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.HableCurve;

public class DrawingManager : MonoBehaviour
{
    public Camera currCamera;
    public int maxNumTrailSegments = 20;
    public float maxSegmentLength = 1f;
    public float segmentWidth = 1f;
    [SerializeField]
    private Vector3 startingPoint = Vector3.zero;
    [SerializeField]
    private List<CatchTrailCollider> segments = new List<CatchTrailCollider>();
    [SerializeField]
    private CatchTrailCollider lastSegment = null;
    private bool clearSegmentsOnNextCycle = false;
    private FishManager fishManager;

    void Start()
    {
        startingPoint = Vector3.zero;
        segments = new List<CatchTrailCollider>();
        fishManager = GetComponent<FishManager>();
    }

    void Update()
    {
        // Methodology:
        // When left mouse is clicked, track where the cursor is on the floor. Save the initial point and create a trigger 
        // As cursor moves, change size of trigger to draw betwen the original point and the cursor.
        // When left click is released, delete all triggers
        if (Input.GetMouseButton(0))
        {
            DrawSegments();
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
    }

    public void TriggerCatchCircleComplete(int firstid, int lastid)
    {
        // Called when triggers detect that there is circle closure.
        // Get the list of segments that  we care about
        var segmentPoints = new List<Vector3>();
        foreach(var segs in segments)
        {
            if(segs.id > lastid)
            {
                // Done
                break;
            }
            if(segs.id >= firstid || segs.id <= lastid)
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
                fish.IncreaseCatchBar();
            }
        }

        // Stage to delete segments since we are done with them now
        clearSegmentsOnNextCycle = true;
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
                // Starting case
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

        CatchTrailCollider ctc = segs.GetComponent<CatchTrailCollider>();
        ctc.startpoint = startpoint;
        ctc.endpoint = endpoint;
    }

    private void MaxSegmentCheck()
    {
        if (segments.Count > maxNumTrailSegments)
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

        startingPoint = Vector3.zero;
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
}
