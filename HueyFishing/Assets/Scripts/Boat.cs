using Unity.VisualScripting;
using UnityEngine;

public class Boat : MonoBehaviour
{

    public float speed;
    private Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            rb.AddForce(Vector3.right * speed);
        }else if (Input.GetKeyDown(KeyCode.S))
        {
            rb.AddForce(Vector3.left * speed);
        }


    }




}
