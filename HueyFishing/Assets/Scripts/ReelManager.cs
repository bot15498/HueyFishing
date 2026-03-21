using System.Collections;
using UnityEngine;

public class ReelManager : MonoBehaviour
{
    public float reelSpeed = 5f;
    public Transform handleTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        handleTransform.eulerAngles = new Vector3(0f, 0f, 0f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateReelPosition(Vector3 lastPos, Vector3 newPos)
    {
        float diff = Vector3.Distance(newPos, lastPos);
        handleTransform.eulerAngles = new Vector3(0f, 0f, handleTransform.eulerAngles.z + reelSpeed * diff);
    }
}
