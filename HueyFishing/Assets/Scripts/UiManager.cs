using UnityEngine;

public class UiManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created+
    public GameObject boatUI;
    public GameObject fishingUI;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startFishingUI()
    {
        //add transition between two uis
        boatUI.SetActive(false);
        fishingUI.SetActive(true);
       
    }

    public void endFishingUI()
    {



    }

}
