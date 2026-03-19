using UnityEngine;

public class UnlockManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public bool Fish1;

    void Start()
    {
        Fish1 = false;

    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void fishUnlock(int fishunlock)
    {
        switch (fishunlock)
        {
            case 1:
                Fish1 = true;
                break;
            /*case 2:
                option2 = true;
                break;
            case 3:
                option3 = true;
                break;*/
        }



    }



}
