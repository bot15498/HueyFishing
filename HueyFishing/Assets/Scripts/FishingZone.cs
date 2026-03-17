using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;

public class FishingZone : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject gamemanager;
    CameraManager cmanager;
    public GameObject boatlocationStorage;
    public CinemachineCamera FishingCamera;
    bool canstartfishing;
    public Transform ParkSpot;
    public float moveDuration = 1f;
    UiManager uiManager;

    Tween moveTween;

    GameObject player;
    void Start()
    {
        canstartfishing = false;
        gamemanager = GameObject.FindGameObjectWithTag("GameManager");
        cmanager = gamemanager.GetComponent<CameraManager>();
        uiManager = gamemanager.GetComponent<UiManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(canstartfishing == true && Input.GetKeyDown(KeyCode.F))
        {
            startFishing();
        }
       
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            player = other.gameObject;
        }
    }



    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            canstartfishing = true;
         
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            canstartfishing = false;
        }
    }

    void startFishing()
    {
        player.GetComponent<Boat>().toggleCanmove();
        cmanager.switchCamera(FishingCamera);
        uiManager.startFishingUI();
        Debug.Log("Now Fishing");
        MoveBoat();


        //add start fishing stuff here

        //battle intro
        
    }

    public void MoveBoat()
    {
        if (ParkSpot == null) return;

        // Keep current Y position
        Vector3 targetPos = new Vector3(
            ParkSpot.position.x,
            player.transform.position.y,
            ParkSpot.position.z
        );

        
        moveTween?.Kill();

        moveTween = player.transform.DOMove(targetPos, moveDuration);
    }
}
