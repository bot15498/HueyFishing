using UnityEngine;

public class FishingZoneManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public FishingZone currentFishingZone;
    public bool isfishing;
    PlayerHealthManager healthManager;

    public Animator winanim;
    public Animator loseanim;

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

        if(playerWin == true)
        {
            winanim.Play("WinAnimation",- 1, 0f);
          
        }

        if(playerWin == false)
        {
            loseanim.Play("LoseAnimation", -1, 0f);
           
        }
        currentFishingZone.StopFishing(playerWin);
        isfishing = false;
        healthManager.resetEnergy();
        currentFishingZone = null;
        
    }
}
