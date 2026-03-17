using UnityEngine;
using UnityEngine.Splines;

public class rippletesting : MonoBehaviour
{

    WaterRippleManager ripplemanager;

    public GameObject testhit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ripplemanager = GetComponent<WaterRippleManager>();
    }

    // Update is called once per frame
    void Update()
    {
        ripplemanager.AddRipple(testhit.transform.position);
    }
}
