using UnityEngine;

public class FishingZoneManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public FishingZone currentFishingZone;
    public bool isfishing;
    PlayerHealthManager healthManager;

    void Start()
    {
        isfishing = false;
        healthManager = GetComponent<PlayerHealthManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void setfishingZone(FishingZone fishingZone)
    {
        currentFishingZone = fishingZone;
        isfishing = true;


    }

    public void clearFishingZone(bool playerWin)
    {
        currentFishingZone.StopFishing(playerWin);
        isfishing = false;
        healthManager.resetEnergy();
        currentFishingZone = null;
        
    }
}
