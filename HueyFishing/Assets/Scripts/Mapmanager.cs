using UnityEngine;

public class Mapmanager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject map;
    public bool isactive;
    public Pagemanager pagemanager;
    public SkillPageManager skillpagemanager;


    void Start()
    {
        map.SetActive(false);
        isactive = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void mapToggle()
    {
        isactive = !isactive;
        map.SetActive(isactive);
        if(skillpagemanager.isbookactive == true)
        {
            skillpagemanager.closeBook();
        }

        if (pagemanager.isbookactive == true)
        {
            pagemanager.closeBook();
        }

    }

    public void closemap()
    {
        isactive=false;
        map.SetActive(false);
    }
}
