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

    public void SpawnFishForRegion(FishingRegion region)
    {
        switch (region)
        {
            case FishingRegion.TimmyRegion:
                // For timmy, area is fixed
                List<Vector3> positions = new List<Vector3> { new Vector3(80f, -0.17f, -14f) };
                List<GameObject> tospawn = new List<GameObject> { timmyFishPrefab };
                StartCoroutine(SpawnFish(tospawn, positions));
                break;
        }
    }

    public void Cleanup()
    {
        StartCoroutine(DoCleanup());
    }

    private IEnumerator DoCleanup()
    {
        if (currentFish.Count > 0)
        {
            // Animation to have fish swim away
            while (currentFish[0].transform.position.y > -1)
            {
                // Have fish sink into the floor
                foreach (var fish in currentFish)
                {
                    var targetPos = fish.transform.position;
                    targetPos.y = -5;
                    fish.transform.position = Vector3.MoveTowards(transform.position, targetPos, 1f * Time.deltaTime);
                }
                yield return null;
            }
        }

        // Delete them
        currentFish.Clear();
    }

    private IEnumerator SpawnFish(List<GameObject> fish, List<Vector3> locations)
    {
        // Animate fish swim in.
        // Just raise them from the bottom
        if (currentFish.Count > 0)
        {
            List<GameObject> fishSpawned = new List<GameObject>();
            for (int i = 0; i < fish.Count; i++)
            {
                fishSpawned.Add(Instantiate(fish[i]));
                var startingPos = locations[i];
                startingPos.y = -5;
                fishSpawned[i].transform.position = startingPos;
            }

            while (currentFish[0].transform.position.y < 0)
            {
                // Have fish raise up.
                for (int i = 0; i < fish.Count; i++)
                {
                    fish[i].transform.position = Vector3.MoveTowards(transform.position, locations[i], 1f * Time.deltaTime);
                }
                yield return null;
            }
        }

        // Fill the current fish list
        currentFish.Clear();
        foreach (var fis in fish)
        {
            currentFish.Add(fis.GetComponent<FishCatchbar>());
        }
    }
}
