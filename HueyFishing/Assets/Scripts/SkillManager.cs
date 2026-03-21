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
    private bool skillIsActive = false;

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
                playerHealthManager.resetEnergy();
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
    private IEnumerator StartInvincible()
    {
        playerHealthManager.isLineUnbreakable = true;
        yield return new WaitForSeconds(invincibleDuration);
        playerHealthManager.isLineUnbreakable = false;
        skillIsActive = false;
    }

    private IEnumerator StartDoubleCatch()
    {
        playerHealthManager.isDoubleCatchRate = true;
        yield return new WaitForSeconds(doubleCatchDuration);
        playerHealthManager.isDoubleCatchRate = false;
        skillIsActive = false;
    }

    private IEnumerator StartLifesteal()
    {
        playerHealthManager.isLifesteal = true;
        yield return new WaitForSeconds(lifestealDuration);
        playerHealthManager.isLifesteal = false;
        skillIsActive = false;
    }

    private IEnumerator StartBubble()
    {
        playerHealthManager.isBubbleStunActive = true;
        yield return new WaitForSeconds(bubbleAbilityDuration);
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
