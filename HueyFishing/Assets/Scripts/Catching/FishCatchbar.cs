using UnityEngine;

public class FishCatchbar : MonoBehaviour
{
    public int currCatchBar = 0;
    public int maxCatchBar = 5;
    public FishManager fishManager;
    public GameObject Sprite;
    public ParticleRingPull particlering;
    public bool isinvincible;

    [Header("Healing")]
    public int healAmount = 1;
    public float healInterval = 1f;

    private float healTimer;

    void Start()
    {
        isinvincible = false;
        currCatchBar = 0;
        healTimer = healInterval;

        if (fishManager == null)
        {
            fishManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<FishManager>();
        }
    }

    void Update()
    {
        if (isinvincible)
        {
            DecreaseCatchBarOverTime();
        }
        else
        {
            // reset timer so it doesn't instantly tick when invincible turns back on
            healTimer = healInterval;
        }
    }

    public void IncreaseCatchBar(int value = 1, bool isFromCircle = true, float loopRadius = 0f)
    {
        if (!isinvincible)
        {
            currCatchBar += value;
            particlering.PlayRing(loopRadius);

            if (!isFromCircle && currCatchBar >= maxCatchBar)
            {
                currCatchBar = maxCatchBar - 1;
            }

            if (currCatchBar >= maxCatchBar)
            {
                Debug.Log("Caught fish");
                fishManager.currentFish.Remove(this);
                Destroy(gameObject);
            }
        }
    }

    public void DecreaseCatchBarOverTime()
    {
        if (currCatchBar <= 0)
            return;

        healTimer -= Time.deltaTime;

        if (healTimer <= 0f)
        {
            currCatchBar -= healAmount;

            if (currCatchBar < 0)
                currCatchBar = 0;

            healTimer = healInterval;
        }
    }
}