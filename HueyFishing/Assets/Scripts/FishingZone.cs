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
    public GameObject boatlocationStorage;
    public CinemachineCamera FishingCamera;
    public FishingRegion regionType;
    bool canstartfishing;
    public bool isFishing = false;
    public bool isSpawningFish = false;
    public Transform ParkSpot;
    public float moveDuration = 1f;
    UiManager uiManager;
    private DrawingManager drawingManager;
    private FishManager fishManager;
    public List<GameObject> fishPrefabs;
    public List<Vector3> fishStartPositions;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (canstartfishing == true && Input.GetKeyDown(KeyCode.F))
        {
            isFishing = true;
            isSpawningFish = true;
            startFishing();
        }
        else if (isFishing && !isSpawningFish && CheckForFishingDone())
        {
            Debug.Log("You caught all the fish");
            isFishing = false;
            StopFishing();
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
            var startingPos = fishStartPositions[i];
            startingPos.y = -5;
            fishSpawned.Add(Instantiate(fishPrefabs[i], startingPos, Quaternion.identity));
        }

        while (fishSpawned[0].transform.position.y < fishStartPositions[0].y - 0.1f)
        {
            // Have fish raise up.
            for (int i = 0; i < fishPrefabs.Count; i++)
            {
                fishSpawned[i].transform.position = Vector3.MoveTowards(fishSpawned[i].transform.position, fishStartPositions[i], 7f * Time.deltaTime);
            }
            yield return null;
        }

        // Fill the current fish list
        fishManager.currentFish.Clear();
        foreach (var fis in fishSpawned)
        {
            fishManager.currentFish.Add(fis.GetComponent<FishCatchbar>());
        }
        isSpawningFish = false;
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
