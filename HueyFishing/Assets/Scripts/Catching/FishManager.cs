using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FishingRegion
{
    TimmyRegion,
    DraculaRegion
}

public class FishManager : MonoBehaviour
{
    public GameObject testFishPrefab;
    public GameObject timmyFishPrefab;
    public List<FishCatchbar> currentFish;

    void Start()
    {
        currentFish = new();
    }

    void Update()
    {

    }

    public void Cleanup()
    {
        StartCoroutine(DoCleanup());
    }

    private IEnumerator DoCleanup()
    {

        // Animation to have fish swim away

        /*while (currentFish[0].transform.position.y > -1)
        {
            // Have fish sink into the floor
            foreach (var fish in currentFish)
            {
                var targetPos = fish.transform.position;
                targetPos.y = -5;
                fish.transform.position = Vector3.MoveTowards(transform.position, targetPos, 1f * Time.deltaTime);
            }

        }*/

        foreach (FishCatchbar obj in currentFish)
        {
            Destroy(obj.gameObject);
        }

        //Destroy(currentFish[0]);
            


        

        // Delete them
        currentFish.Clear();
         yield return null;
    }

    
}
