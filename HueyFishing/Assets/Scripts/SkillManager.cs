using System.Collections;
using UnityEngine;

public enum SkillType
{
    BubbleStun,
    DoubleCatch,
    Invincible,
    Lifesteal
}

public class SkillManager : MonoBehaviour
{
    [Header("Stun Params")]
    public float bubbleAbilityDuration = 5f;
    public float bubbleSpeed = 5f;
    public int bubbleDamage = 10;
    public GameObject BubblePrefab;
    [Header("Invincible Params")]
    public float invincibleDuration = 5f;
    [Header("Double Catch Params")]
    public float doubleCatchDuration = 5f;
    [Header("Lifesteal Params")]
    public float lifestealDuration = 5f;
    public int lifestealAmountPerCircle = 10;

    private PlayerHealthManager playerHealthManager;
    public bool skillIsActive = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerHealthManager = GetComponent<PlayerHealthManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ActivateSkill(int skillid)
    {
        if (!skillIsActive)
        {

            if (playerHealthManager.currBarGuage >= 100)
            {
                // playerHealthManager.resetEnergy();
                skillIsActive = true;
                switch (skillid)
                {
                    case 1:
                        StartCoroutine(StartBubble());
                        break;
                    case 2:
                        StartCoroutine(StartDoubleCatch());
                        break;
                    case 3:
                        StartCoroutine(StartInvincible());
                        break;
                    case 4:
                        StartCoroutine(StartLifesteal());
                        break;
                }
            }
        }
    }

    // The correct way to do this is to pass a method in as a parameter, but this is to make it easier to Read and debug
    // For each skill, use the expected duration, drain the current guage bar at that rate. If they increase the guage during that time then it just increases their chances
    private IEnumerator StartInvincible()
    {
        playerHealthManager.isLineUnbreakable = true;
        float delta = playerHealthManager.currBarGuage / invincibleDuration;
        for(float time=0; time < invincibleDuration; time += Time.deltaTime)
        {
            yield return null;
            playerHealthManager.currBarGuage = playerHealthManager.currBarGuage - Time.deltaTime * delta;
            playerHealthManager.currBarGuage = playerHealthManager.currBarGuage <= 0 ? 0 : playerHealthManager.currBarGuage;
        }
        playerHealthManager.isLineUnbreakable = false;
        skillIsActive = false;
    }

    private IEnumerator StartDoubleCatch()
    {
        playerHealthManager.isDoubleCatchRate = true;
        float delta = playerHealthManager.currBarGuage / doubleCatchDuration;
        for(float time=0; time < doubleCatchDuration; time += Time.deltaTime)
        {
            yield return null;
            playerHealthManager.currBarGuage = playerHealthManager.currBarGuage - Time.deltaTime * delta;
            playerHealthManager.currBarGuage = playerHealthManager.currBarGuage <= 0 ? 0 : playerHealthManager.currBarGuage;
        }
        playerHealthManager.isDoubleCatchRate = false;
        skillIsActive = false;
    }

    private IEnumerator StartLifesteal()
    {
        playerHealthManager.isLifesteal = true;
        float delta = playerHealthManager.currBarGuage / lifestealDuration;
        for(float time=0; time < lifestealDuration; time += Time.deltaTime)
        {
            yield return null;
            playerHealthManager.currBarGuage = playerHealthManager.currBarGuage - Time.deltaTime * delta;
            playerHealthManager.currBarGuage = playerHealthManager.currBarGuage <= 0 ? 0 : playerHealthManager.currBarGuage;
        }
        playerHealthManager.isLifesteal = false;
        skillIsActive = false;
    }

    private IEnumerator StartBubble()
    {
        playerHealthManager.isBubbleStunActive = true;
        float delta = playerHealthManager.currBarGuage / bubbleAbilityDuration;
        for(float time=0; time < bubbleAbilityDuration; time += Time.deltaTime)
        {
            yield return null;
            playerHealthManager.currBarGuage = playerHealthManager.currBarGuage - Time.deltaTime * delta;
            playerHealthManager.currBarGuage = playerHealthManager.currBarGuage <= 0 ? 0 : playerHealthManager.currBarGuage;
        }
        playerHealthManager.isBubbleStunActive = false;
        skillIsActive = false;
    }

    public void CreateBubbleCheck(Vector3 startPos, Vector3 endPos)
    {
        // Only spawn bubble if skill is active
        if (playerHealthManager.isBubbleStunActive)
        {
            GameObject bubble = Instantiate(BubblePrefab, startPos, Quaternion.LookRotation(endPos));
            StunBubble bub = bubble.GetComponent<StunBubble>();
            bub.SetVelocity((endPos - startPos).normalized * bubbleSpeed);
            bub.SetDamageAmount(bubbleDamage);
        }
    }
}
