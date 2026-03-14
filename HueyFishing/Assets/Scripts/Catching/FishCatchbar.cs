using NUnit.Framework;
using UnityEngine;

public class FishCatchbar : MonoBehaviour
{
    public int currCatchBar = 0;
    public int maxCatchBar = 5;

    void Start()
    {
        currCatchBar = 0;
    }

    void Update()
    {

    }

    public void IncreaseCatchBar(int value = 1)
    {
        currCatchBar += value;
        if (currCatchBar >= maxCatchBar)
        {
            // you win!
            Debug.Log("Caught fish");
            Destroy(gameObject);
        }
    }
}
