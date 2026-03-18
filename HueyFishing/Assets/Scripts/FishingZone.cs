using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using System.Collections;

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
    private DrawingManager drawingManager;

    Tween moveTween;

    GameObject player;

    public float delayTime;
    void Start()
    {
        canstartfishing = false;
        gamemanager = GameObject.FindGameObjectWithTag("GameManager");
        cmanager = gamemanager.GetComponent<CameraManager>();
        uiManager = gamemanager.GetComponent<UiManager>();
        drawingManager = gamemanager.GetComponent<DrawingManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (canstartfishing == true && Input.GetKeyDown(KeyCode.F))
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

        //fihsing zone intro and delay
        //disable starting fishing zone mesh and trigger collider

        StartCoroutine(startFishingGameplay(delayTime));


        

        //battle intro

    }

    public void StopFishing()
    {
        // Return camera to normal
        player.GetComponent<Boat>().toggleCanmove();
        cmanager.switchCamera(cmanager.boatCamera);
        uiManager.endFishingUI();

        drawingManager.canDraw = false;
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


    private IEnumerator startFishingGameplay(float delayTime)
    {
        //Wait for the specified delay time before continuing.
        yield return new WaitForSeconds(delayTime);

        Debug.Log("delay");
        //add start fishing stuff here
        //enable fishing hud and animation. 
        drawingManager.canDraw = true;

        //Do the action after the delay time has finished.
    }
}
