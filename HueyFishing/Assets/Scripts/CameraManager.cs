using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public CinemachineCamera boatCamera;
    public CinemachineCamera previousCamera;
    public CinemachineCamera currentcamera;

    void Start()
    {
        currentcamera = boatCamera;
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void switchCamera(CinemachineCamera newcamera)
    {
        previousCamera = currentcamera;
        currentcamera = newcamera;
        previousCamera.Priority = 10;
        currentcamera.Priority = 20;


    }

    public void returntoboat()
    {
        currentcamera = boatCamera;
        boatCamera.Priority = 20;
    }

}
