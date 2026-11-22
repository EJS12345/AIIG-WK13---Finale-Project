using UnityEngine;

public class EatFoodAction : GoapAction
{
    private bool ate = false;
    private SupplyPileComponent targetSupply;
    private float startTime = 0f;
    public float eatDuration = 1.5f;

    public EatFoodAction()
    {
        addPrecondition("isHungry", true);
        addEffect("isFed", true);
        cost = 1.0f;
    }

    public override void reset()
    {
        ate = false;
        targetSupply = null;
        startTime = 0f;
    }

    public override bool isDone() => ate;
    public override bool requiresInRange() => true;

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        SupplyPileComponent[] supplies = Object.FindObjectsByType<SupplyPileComponent>(FindObjectsSortMode.None);
        SupplyPileComponent closest = null;
        float closestDist = Mathf.Infinity;

        foreach (SupplyPileComponent supply in supplies)
        {
            float dist = Vector3.Distance(supply.transform.position, agent.transform.position);
            if (dist < closestDist)
            {
                closest = supply;
                closestDist = dist;
            }
        }

        if (closest == null) return false;

        targetSupply = closest;
        target = targetSupply.gameObject;
        return true;
    }

    public override bool perform(GameObject agent)
    {
        if (startTime == 0) startTime = Time.time;

        if (Time.time - startTime > eatDuration)
        {
            Labourer labourer = agent.GetComponent<Labourer>();
            if (labourer != null)
            {
                labourer.hunger = 0f; // Reset hunger state
            }
            ate = true;
        }
        return true;
    }
}