using UnityEngine;

public class PlayerHealthManager : MonoBehaviour
{
    [Header("Main Player Stats")]
    public int currHealth = 100;
    public int maxHealth = 100;
    public int currBarGuage = 100;
    public int maxBarGuage = 100;
    public int abilityCost = 25;
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

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // wwwwwww lazy approach best approach
        circlePower = isDoubleCatchRate ? baseCirclePower * 2 : baseCirclePower;
        maxLineLength = isLongerLine ? Mathf.RoundToInt(baseMaxLineLength * 1.5f) : baseMaxLineLength;
        //segmentTimeoutTime = isLongerLine ? baseSegmentTimeoutTime * 1.5f : baseSegmentTimeoutTime;
    }

    public void DoDamageToPlayer(int damage)
    {
        if (!isLineUnbreakable || damage < 0)
        {
            currHealth -= damage;
            if (currHealth <= 0)
            {
                // die
                Debug.Log("Player died");
            }
        }
    }

    public void AddPlayerAbilityGuage(int value)
    {
        currBarGuage = Mathf.Clamp(currBarGuage + value, 0, maxBarGuage);
    }
}
