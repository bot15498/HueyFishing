using UnityEngine;

public class Pagemanager : MonoBehaviour
{
    [Header("Pages in order")]

    public GameObject Book;
    public GameObject[] pages;
    public GameObject nextPageButton;
    public GameObject prevPageButton;


    private int currentPage = 0;

    public bool isbookactive;

    public SkillPageManager skillsPage;
    public Mapmanager mapmanager;

    void Start()
    {
        Book.SetActive(false);
        isbookactive = false;
        ShowPage(currentPage);
        if(currentPage == 0)
        {
            prevPageButton.SetActive(false);
        }
    }

    public void NextPage()
    {
       
        if (pages.Length == 0) return;

        currentPage++;

        if (currentPage >= pages.Length)
            currentPage = pages.Length - 1; // stops at last page

        ShowPage(currentPage);
        if(currentPage != 0)
        {
            prevPageButton.SetActive(true);
            
        }

        if(currentPage == pages.Length -1)
        {
           
            nextPageButton.SetActive(false);
        }
    }

    public void PreviousPage()
    {
        if (pages.Length == 0) return;

        currentPage--;

        if (currentPage < 0)
            currentPage = 0; // stops at first page

        ShowPage(currentPage);

        if(currentPage == 0)
        {
            prevPageButton.SetActive(false);
        }
        if(currentPage != pages.Length - 1)
        {
            nextPageButton.SetActive(true);
        }



    }

    void ShowPage(int pageIndex)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == pageIndex);
        }
    }


    public void togglebook()
    {
        isbookactive = !isbookactive;
        Book.SetActive(isbookactive);

        if (skillsPage.isbookactive == true)
        {
            skillsPage.closeBook();
        }

        if(mapmanager.isactive == true)
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
