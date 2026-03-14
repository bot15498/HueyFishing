using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestLoadAdditiveScene : MonoBehaviour
{
    private bool catchingSceneLoaded = false;
    private bool catchingSceneIsLoading = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.O) && !catchingSceneLoaded && !catchingSceneIsLoading)
        {
            StartCoroutine(LoadCatchingScene());
        }
        else if (Input.GetKeyUp(KeyCode.O) && catchingSceneLoaded && !catchingSceneIsLoading)
        {
            StartCoroutine(UnloadCatchingScene());
        }
    }

    private IEnumerator LoadCatchingScene()
    {
        catchingSceneIsLoading = true;
        var loader = SceneManager.LoadSceneAsync("CatchingScene", LoadSceneMode.Additive);
        while (!loader.isDone)
        {
            yield return null;
        }
        catchingSceneLoaded = true;
        catchingSceneIsLoading = false;
    }

    private IEnumerator UnloadCatchingScene()
    {
        catchingSceneIsLoading = true;
        var loader = SceneManager.UnloadSceneAsync("CatchingScene");
        while (!loader.isDone)
        {
            yield return null;
        }
        catchingSceneLoaded = false;
        catchingSceneIsLoading = false;
    }
}
