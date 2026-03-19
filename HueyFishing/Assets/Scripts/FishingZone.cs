using Unity.Cinemachine;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class FishingZone : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject gamemanager;
    CameraManager cmanager;
    public CinemachineCamera FishingCamera;
    public FishingRegion regionType;
    public bool canstartfishing;
    public bool isFishing = false;
    public bool isSpawningFish = false;
    public Transform ParkSpot;
    public float moveDuration = 1f;
    UiManager uiManager;
    private DrawingManager drawingManager;
    private FishManager fishManager;
    public List<GameObject> fishPrefabs;
  
    public Collider thisCollider;
    FishingZoneManager fishingzoneManager;
    FishingZone thisFishingZone;
    UnlockManager unlockManager;
    public GameObject[] FishSpawns;

    public int fishingUnlockID;

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
        fishManager = gamemanager.GetComponent<FishManager>();
        thisCollider = gameObject.GetComponent<Collider>();
        fishingzoneManager = gamemanager.GetComponent<FishingZoneManager>();
        thisFishingZone = gameObject.GetComponent<FishingZone>();
        unlockManager = gamemanager.GetComponent<UnlockManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (canstartfishing == true && Input.GetKeyDown(KeyCode.F))
        {
            isFishing = true;
            isSpawningFish = true;
            startFishing();
            canstartfishing = false;
        }
        else if (isFishing && !isSpawningFish && CheckForFishingDone())
        {


            Debug.Log("You caught all the fish");

            fishingzoneManager.clearFishingZone(true);
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
        thisCollider.enabled = false;
        fishingzoneManager.setfishingZone(thisFishingZone);

        //fihsing zone intro and delay
        //disable starting fishing zone mesh and trigger collider

        StartCoroutine(startFishingGameplay(delayTime));




        //battle intro

    }

    public void StopFishing(bool playerWin)
    {
        // Return camera to normal
        player.GetComponent<Boat>().toggleCanmove();
        cmanager.switchCamera(cmanager.boatCamera);
        uiManager.endFishingUI();

        thisCollider.enabled = true;

        drawingManager.canDraw = false;
        fishManager.Cleanup();
        isFishing = false;
        if (playerWin == true)
        {
            //trigger any win stuff here
            unlockManager.fishUnlock(fishingUnlockID);

            Debug.Log("fishwin");
            gameObject.SetActive(false);

        }

        //canstartfishing = true;

        

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
        StartCoroutine(SpawnFish());

        //Do the action after the delay time has finished.
    }

    private IEnumerator SpawnFish()
    {
        isSpawningFish = true;
        // Animate fish swim in.
        // Just raise them from the bottom

        List<GameObject> fishSpawned = new List<GameObject>();
        for (int i = 0; i < fishPrefabs.Count; i++)
        {
            
            fishSpawned.Add(Instantiate(fishPrefabs[i], FishSpawns[i].transform.position, Quaternion.identity));
            fishSpawned[i].GetComponent<FishMovement>().fishSpawnCenter = FishSpawns[i];

        }

        /*while (fishSpawned[0].transform.position.y < fishStartPositions[0].y - 0.1f)
        {
            // Have fish raise up.
            for (int i = 0; i < fishPrefabs.Count; i++)
            {
                fishSpawned[i].transform.position = Vector3.MoveTowards(fishSpawned[i].transform.position, fishStartPositions[i], 7f * Time.deltaTime);
            }
           
        }*/
        
        // Fill the current fish list
        fishManager.currentFish.Clear();
        foreach (var fis in fishSpawned)
        {
            fishManager.currentFish.Add(fis.GetComponent<FishCatchbar>());

        }

        isSpawningFish = false;

        yield return null;
    }

    private bool CheckForFishingDone()
    {
        
        
        bool done = true;
        foreach(var fish in fishManager.currentFish)
        {
            if (fish != null)
            {
                done = false;
            }
        }
        return done;
    }





}
