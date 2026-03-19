using UnityEngine;

public class FishingZoneManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public FishingZone currentFishingZone;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void setfishingZone(FishingZone fishingZone)
    {
        currentFishingZone = fishingZone;

    }

    public void clearFishingZone(bool playerWin)
    {
        currentFishingZone.StopFishing(playerWin);

        currentFishingZone = null;
    }
}
