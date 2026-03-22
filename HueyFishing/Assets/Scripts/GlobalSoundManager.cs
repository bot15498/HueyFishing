using System.Collections;
using UnityEngine;

public class GlobalSoundManager : MonoBehaviour
{
    public AudioSource reelSource;
    public AudioSource effectSource;
    public AudioSource boatSource;
    public AudioSource catchSource;
    public AudioClip lineSnap;
    public AudioClip reelSound;
    public AudioClip boatSound;
    public AudioClip catchSound;
    private bool isStoppingBoatAudio = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        reelSource.loop = true;
        reelSource.clip = reelSound;
        boatSource.loop = true;
        boatSource.clip = boatSound;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayLineSnap()
    {
        effectSource.PlayOneShot(lineSnap);
    }

    public void PlayReelSound()
    {
        if (!reelSource.isPlaying)
        {
            reelSource.Play();
            Debug.Log("looping reel");
        }
    }

    public void StopReelSound()
    {
        if (reelSource.isPlaying)
        {
            reelSource.Stop();
            Debug.Log("stopping reel");
        }
    }

    public void PlayCatchSound()
    {
        catchSource.PlayOneShot(catchSound);
    }

    public void SetReelSpeed(float speed)
    {
        // 1 means normal speed, anything higher is faster.
    }

    public void PlayBoatSound()
    {
        if (!boatSource.isPlaying)
        {
            boatSource.Play();
        }
    }

    public void StopBoatSound()
    {
        if (boatSource.isPlaying && !isStoppingBoatAudio)
        {
            // boatSource.Stop();
            StartCoroutine(FadeOutBoatSound());
        }
    }

    private IEnumerator FadeOutBoatSound()
    {
        isStoppingBoatAudio = true;
        float startVolume = boatSource.volume;
        while (boatSource.volume > 0)
        {
            boatSource.volume -= startVolume * Time.deltaTime;
            yield return null;
        }
        boatSource.Stop();
        boatSource.volume = startVolume;
        isStoppingBoatAudio = false;
    }
}
