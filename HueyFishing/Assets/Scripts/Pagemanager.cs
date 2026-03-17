using UnityEngine;

public class Pagemanager : MonoBehaviour
{
    [Header("Pages in order")]
    public GameObject[] pages;
    public GameObject nextPageButton;
    public GameObject prevPageButton;
    private int currentPage = 0;

    void Start()
    {
        ShowPage(currentPage);
    }

    public void NextPage()
    {
        if (pages.Length == 0) return;

        currentPage++;

        if (currentPage >= pages.Length)
            currentPage = pages.Length - 1; // stops at last page

        ShowPage(currentPage);
    }

    public void PreviousPage()
    {
        if (pages.Length == 0) return;

        currentPage--;

        if (currentPage < 0)
            currentPage = 0; // stops at first page

        ShowPage(currentPage);
    }

    void ShowPage(int pageIndex)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == pageIndex);
        }
    }
}
