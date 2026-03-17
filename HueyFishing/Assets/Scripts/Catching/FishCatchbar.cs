using NUnit.Framework;
using UnityEngine;

public class FishCatchbar : MonoBehaviour
{
    public int currCatchBar = 0;
    public int maxCatchBar = 5;
    public FishManager fishManager;

    void Start()
    {
        currCatchBar = 0;
    }

    void Update()
    {

    }

    public void IncreaseCatchBar(int value = 1, bool isFromCircle=true)
    {
        currCatchBar += value;
        if (!isFromCircle && currCatchBar >= maxCatchBar)
        {
            // Need to do a circle around a fish for the final catch, even if bar is full
            // Set bar back to 1 minus
            currCatchBar = maxCatchBar - 1;
        }

        if (currCatchBar >= maxCatchBar)
        {
            // you win!
            Debug.Log("Caught fish");
            fishManager.currentFish.Remove(this);
            Destroy(gameObject);
        }
    }
}
