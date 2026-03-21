using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthManager : MonoBehaviour
{
    [Header("Main Player Stats")]
    public int currHealth = 100;
    public int maxHealth = 100;
    public float currBarGuage = 0;
    public float maxBarGuage = 100;
    public float abilityCost = 25;
    [Header("Other Player Stats")]
    public int baseCirclePower = 100; // How much catch bar you fill up per circle. 
    public int circlePower = 100;
    public int baseMaxLineLength = 60;
    public int maxLineLength = 60;
    public float baseSegmentTimeoutTime = 3f;
    public float segmentTimeoutTime = 3f;
    [Header("Player Upgrades")]
    public bool isLineUnbreakable = false;
    public bool isDoubleCatchRate = false;
    public bool isLongerLine = false;
    public bool isShootingProjectiles = false;
    public bool isLifesteal = false;
    public bool isBubbleStunActive = false;
    [Header("Damage Allowance")]
    public bool canBeDamaged = true;
    public float damageResetTime = 0.1f;

    private FishManager fishManager;
    private DrawingManager drawManager;
    private UiManager uiManager;
    private CameraManager cmanager;
    private GameObject player;
    private SkillManager skillManager;
    FishingZoneManager fishingZoneManager;

    public float EnergyRegenRate;


    void Start()
    {
        fishManager = GetComponent<FishManager>();
        uiManager = GetComponent<UiManager>();
        cmanager = GetComponent<CameraManager>();
        drawManager = GetComponent<DrawingManager>();
        player = GameObject.FindGameObjectWithTag("Player");
        fishingZoneManager = gameObject.GetComponent<FishingZoneManager>();
        skillManager = gameObject.GetComponent<SkillManager>();

        currHealth = maxHealth;
        currBarGuage = 0;



    }

    // Update is called once per frame
    void Update()
    {
        // wwwwwww lazy approach best approach
        circlePower = isDoubleCatchRate ? baseCirclePower * 2 : baseCirclePower;
        maxLineLength = isLongerLine ? Mathf.RoundToInt(baseMaxLineLength * 1.5f) : baseMaxLineLength;
        //segmentTimeoutTime = isLongerLine ? baseSegmentTimeoutTime * 1.5f : baseSegmentTimeoutTime;

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        if(fishingZoneManager.isfishing == true)
        {
            if(!skillManager.skillIsActive && currBarGuage < maxBarGuage)
            {
                currBarGuage += EnergyRegenRate * Time.deltaTime;

            }


        }

    }

    public void resetEnergy()
    {
        currBarGuage = 0;
    }

    public void addenergy()
    {

        if (currBarGuage < maxBarGuage)
        {
            currBarGuage += 10;
        }
    }
        

    public void DoDamageToPlayer(int damage)
    {
        if (canBeDamaged && (!isLineUnbreakable || damage < 0 ))
        {
            canBeDamaged = false;
            currHealth -= damage;
            if (currHealth <= 0)
            {
                // die
                Debug.Log("Player died");
                FinishFishing();
            }
            StartCoroutine(DelayBeforeAllowingDamageAgain());
        }
    }

    public void AddPlayerAbilityGuage(int value)
    {
        currBarGuage = Mathf.Clamp(currBarGuage + value, 0, maxBarGuage);
    }

    public void FinishFishing()
    {
        fishingZoneManager.clearFishingZone(false);

        resethealth();
    }

    public void resethealth()
    {
        currHealth = maxHealth;
        currBarGuage = 0;
    }

    public void TriggerHealOnCircle()
    {
        currHealth += skillManager.lifestealAmountPerCircle;
        currHealth = currHealth > maxHealth ? maxHealth : currHealth;
    }

    private IEnumerator DelayBeforeAllowingDamageAgain()
    {
        yield return new WaitForSeconds(damageResetTime);
        canBeDamaged = true;
    }
}
