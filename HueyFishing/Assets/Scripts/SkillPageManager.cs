using UnityEngine;

public class SkillPageManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject Book;
    public GameObject[] pages;
    public GameObject mapScreen;
    public Pagemanager Bookscreen;
    public Mapmanager mapmanager;

    private int currentPage = 0;
    public bool isbookactive;

    void Start()
    {
        Book.SetActive(false);
        isbookactive = false;
        ShowPage(currentPage);
    
     
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowPage(int pageIndex)
    {
        pages[currentPage].SetActive(false);

        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == pageIndex);
        }

        currentPage = pageIndex;
    }


    public void togglebook()
    {
        isbookactive = !isbookactive;
        Book.SetActive(isbookactive);
        if (Bookscreen.isbookactive == true)
        {
            Bookscreen.closeBook();
        }

        if (mapmanager.isactive == true)
        {
            mapmanager.closemap();
        }
      
    }


    public void closeBook()
    {
        isbookactive = false;
        Book.SetActive(false);
    }
}
