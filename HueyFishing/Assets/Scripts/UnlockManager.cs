using UnityEngine;

public class UnlockManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public bool Fish0;
    public bool Fish1;
    public bool Fish2;
    public bool Fish3;
    public bool Fish4;

    public GameObject skill1;
    public GameObject skill2;
    public GameObject skill3;
    public GameObject skill4;

    public GameObject Tutorialwalls;

    public GameObject winner;
    void Start()
    {
        winner.SetActive(false);
        Fish1 = false;
        skill1.SetActive(false);
        skill2.SetActive(false);
        skill3.SetActive(false);
        skill4.SetActive(false);


    }

    // Update is called once per frame
    void Update()
    {
        if(Fish1 == true && Fish2 == true && Fish0 == true && Fish3 == true && Fish4 == true)
        {
            winner.SetActive(true);
        }



    }



    public void fishUnlock(int fishunlock)
    {
        switch (fishunlock)
        {
            case 0:
                Fish0 = true;
                Tutorialwalls.SetActive(false);


                break;
            case 1:
                Fish1 = true;
                skill1.SetActive(true);

                break;
            case 2:
                Fish2 = true;
                skill2.SetActive(true);

                break;
            case 3:
                Fish3 = true;
                skill3.SetActive(true);

                break;
            case 4:
                Fish4 = true;
                skill4.SetActive(true);

                break;



        }



    }



}
