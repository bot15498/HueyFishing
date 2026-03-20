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
    public float stunAbilityDuration = 5f;
    public float stunDuration = 1.5f;
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

    public void ActivateSkill(SkillType type)
    {
        if (!skillIsActive)
        {
            skillIsActive = true;
            switch (type)
            {
                case SkillType.BubbleStun:
                    break;
                case SkillType.DoubleCatch:
                    break;
                case SkillType.Invincible:
                    break;
                case SkillType.Lifesteal:
                    break;
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

    public void CreateBubbleCheck(Vector3 position, Vector3 direction)
    {
        if (playerHealthManager.isBubbleStunActive)
        {
            GameObject bubble = Instantiate(BubblePrefab, position, Quaternion.LookRotation(direction));
        }
    }
}
