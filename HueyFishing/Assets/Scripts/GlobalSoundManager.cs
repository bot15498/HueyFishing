using UnityEngine;

public class GlobalSoundManager : MonoBehaviour
{
    public AudioSource reelSource;
    public AudioSource effectSource;
    public AudioClip lineSnap;
    public AudioClip reelSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        reelSource.loop = true;
        reelSource.clip = reelSound;
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
        reelSource.Stop();
        Debug.Log("stopping reel");
    }

    public void SetReelSpeed(float speed)
    {
        // 1 means normal speed, anything higher is faster.
    }
}
