using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created+
    public TMP_Text HealthText;
    public TMP_Text EnergyText;
    public TMP_Text CatchText;

    public GameObject boatUI;
    public GameObject fishingUI;

    public Image energyFill;


    [Header("References")]
    public PlayerHealthManager playerHealth;

    [Tooltip("The front bar that matches health immediately.")]
    public Image instantFill;

    [Tooltip("The back bar that drains down after damage.")]
    public Image delayedFill;

    [Header("Drain Settings")]
    public float delayBeforeDrain = 0.25f;
    public float drainSpeed = 1.5f;

    public float delayBeforeDrainFish = 0.25f;
    public float drainSpeedFish = 1.5f;

    private float drainTimer;
    private float drainTimer2;
    private bool waitingToDrain;

    private bool waitingToDrainFish;

    private float previousHealth;

    private float previousFishCatch;
    public Image instantFishFill;
    public Image delayedFishFill;
    FishManager fishManager;

    void Start()
    {
        
        fishManager = GetComponent<FishManager>();
        playerHealth = GetComponent<PlayerHealthManager>();

        energyFill.fillAmount = playerHealth.currBarGuage/playerHealth.maxBarGuage;
        if (playerHealth == null)
        {
            Debug.LogError("HealthBarController: no healthScript assigned.");
            return;
        }
       

        float normalizedHealth = playerHealth.currHealth / playerHealth.maxHealth;
        instantFill.fillAmount = normalizedHealth;
        delayedFill.fillAmount = normalizedHealth; 
        previousHealth = playerHealth.currHealth;


        previousFishCatch = 0;

        float startFill = 0;
        instantFishFill.fillAmount = startFill;
        delayedFishFill.fillAmount = startFill;
    }

    void Update()
    {
        if (fishManager.currentFish != null && fishManager.currentFish.Count > 0)
        {
            float currentFishCatch = fishManager.currentFish[0].currCatchBar;
            float maxFishCatch = fishManager.currentFish[0].maxCatchBar;
            float targetFishFill = currentFishCatch / maxFishCatch;

            // Front fish bar always smoothly moves toward target
            instantFishFill.fillAmount = Mathf.MoveTowards(
                instantFishFill.fillAmount,
                targetFishFill,
                drainSpeedFish * Time.deltaTime
            );

            // Back fish bar behavior:
            // when catch decreases, drop immediately
            // when catch increases, lag behind and fill up over time
            if (targetFishFill < delayedFishFill.fillAmount)
            {
                delayedFishFill.fillAmount = targetFishFill;
            }
            else
            {
                delayedFishFill.fillAmount = Mathf.MoveTowards(
                    delayedFishFill.fillAmount,
                    targetFishFill,
                    drainSpeedFish * Time.deltaTime
                );
            }

            previousFishCatch = currentFishCatch;
        }
        else
        {
            instantFishFill.fillAmount = 0f;
            delayedFishFill.fillAmount = 0f;
            previousFishCatch = 0f;
        }

        energyFill.fillAmount = playerHealth.currBarGuage / playerHealth.maxBarGuage;

        if (playerHealth == null) return;

        float current = playerHealth.currHealth;
        float max = playerHealth.maxHealth;
        float targetFill = current / max;

        // Damage happened
        if (current < previousHealth)
        {
            instantFill.fillAmount = targetFill;
            waitingToDrain = true;
            drainTimer = delayBeforeDrain;
        }
        // Healing happened
        else if (current > previousHealth)
        {
            instantFill.fillAmount = targetFill;
            delayedFill.fillAmount = targetFill;
            waitingToDrain = false;
        }

        // Drain delayed bar after taking damage
        if (delayedFill.fillAmount > instantFill.fillAmount)
        {
            if (waitingToDrain)
            {
                drainTimer -= Time.deltaTime;
                if (drainTimer <= 0f)
                {
                    waitingToDrain = false;
                }
            }
            else
            {
                delayedFill.fillAmount = Mathf.MoveTowards(
                    delayedFill.fillAmount,
                    instantFill.fillAmount,
                    drainSpeed * Time.deltaTime
                );
            }
        }
        else if (delayedFill.fillAmount < instantFill.fillAmount)
        {
            delayedFill.fillAmount = instantFill.fillAmount;
        }

        previousHealth = current;



        HealthText.text = playerHealth.currHealth + "/" + playerHealth.maxHealth;
        EnergyText.text = (playerHealth.currBarGuage / playerHealth.maxBarGuage).ToString("F1");
        
    }

    public void startFishingUI()
    {
        //add transition between two uis
        boatUI.SetActive(false);
        fishingUI.SetActive(true);
       
    }

    public void endFishingUI()
    {
        boatUI.SetActive(true);
        fishingUI.SetActive(false);


    }

    

}
