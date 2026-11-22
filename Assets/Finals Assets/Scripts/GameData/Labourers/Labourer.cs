using UnityEngine;
using System.Collections.Generic;

public abstract class Labourer : MonoBehaviour, IGoap
{
    public BackpackComponent backpack;
    public float moveSpeed = 3f;

    [Header("Survival Mechanics (Goal 2)")]
    public float hunger = 0f;
    public float maxHunger = 100f;
    public float hungerRate = 5f;

    protected virtual void Start()
    {
        if (backpack == null)
            backpack = gameObject.AddComponent<BackpackComponent>();

        if (backpack.tool == null && !string.IsNullOrEmpty(backpack.toolType))
        {
            GameObject prefab = Resources.Load<GameObject>(backpack.toolType);
            if (prefab != null)
            {
                GameObject tool = Instantiate(prefab, transform.position, transform.rotation);
                backpack.tool = tool;
                tool.transform.parent = transform;
            }
        }
    }

    protected virtual void Update()
    {
        // Increase hunger over time
        hunger += hungerRate * Time.deltaTime;
    }

    public HashSet<KeyValuePair<string, object>> getWorldState()
    {
        HashSet<KeyValuePair<string, object>> worldData = new HashSet<KeyValuePair<string, object>>();

        worldData.Add(new KeyValuePair<string, object>("hasOre", (backpack.numOre > 0)));
        worldData.Add(new KeyValuePair<string, object>("hasLogs", (backpack.numLogs > 0)));
        worldData.Add(new KeyValuePair<string, object>("hasFirewood", (backpack.numFirewood > 0)));
        worldData.Add(new KeyValuePair<string, object>("hasTool", (backpack.tool != null)));

        // World state flags for hunger and food
        worldData.Add(new KeyValuePair<string, object>("isHungry", (hunger >= 50f)));
        worldData.Add(new KeyValuePair<string, object>("isFed", (hunger < 20f)));

        return worldData;
    }

    public abstract HashSet<KeyValuePair<string, object>> createGoalState();

    public void planFailed(HashSet<KeyValuePair<string, object>> failedGoal)
    {
        Debug.LogWarning("<color=orange>Plan Failed for agent:</color> " + gameObject.name);
        SimulationUIManager.LogAction($"<color=orange>{gameObject.name}</color> plan failed!");
    }

    public void planFound(HashSet<KeyValuePair<string, object>> goal, Queue<GoapAction> actions)
    {
        Debug.Log("<color=green>Plan found:</color> " + GoapAgent.prettyPrint(actions));

        // Format clean goal titles for UI log
        string goalName = "Work Job";
        foreach (var kvp in goal)
        {
            if (kvp.Key == "isFed") goalName = "Survival (Eat)";
            else if (kvp.Key == "collectLogs") goalName = "Chop Logs";
            else if (kvp.Key == "collectOre") goalName = "Mine Ore";
            else if (kvp.Key == "collectFirewood") goalName = "Cut Firewood";
            else if (kvp.Key == "collectTools") goalName = "Forge Tools";
        }

        // Push formatted event into synchronized stream
        SimulationUIManager.LogAction($"<b>{gameObject.name}</b> planned: <color=cyan>{goalName}</color>");
    }

    public void actionsFinished()
    {
        Debug.Log("<color=blue>Actions completed</color>");
    }

    public void planAborted(GoapAction aborter)
    {
        Debug.LogWarning("<color=red>Plan Aborted:</color> " + GoapAgent.prettyPrint(aborter));

        string actionName = aborter != null ? aborter.GetType().Name : "Action";
        SimulationUIManager.LogAction($"<color=red>{gameObject.name}</color> aborted plan at {actionName}");
    }

    public bool moveAgent(GoapAction nextAction)
    {
        if (nextAction.target == null) return false;

        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, nextAction.target.transform.position, step);

        // Threshold check prevents infinite stuck loop
        if (Vector3.Distance(transform.position, nextAction.target.transform.position) < 0.2f)
        {
            transform.position = nextAction.target.transform.position;
            nextAction.setInRange(true);
            return true;
        }

        return false;
    }
}