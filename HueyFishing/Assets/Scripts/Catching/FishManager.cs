using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class FishManager : MonoBehaviour
{
    public GameObject testFishPrefab;
    public List<FishCatchbar> currentFish = new List<FishCatchbar>();

    void Start()
    {
        GameObject fish = Instantiate(testFishPrefab);
        FishCatchbar newfish = fish.GetComponent<FishCatchbar>();
        currentFish.Add(newfish);
        newfish.fishManager = this;
    }

    // Update is called once per frame
    void Update()
    {
        if(currentFish.Count == 0)
        {
            GameObject fish = Instantiate(testFishPrefab);
            FishCatchbar newfish = fish.GetComponent<FishCatchbar>();
            currentFish.Add(newfish);
            newfish.fishManager = this;
        }
    }
}
