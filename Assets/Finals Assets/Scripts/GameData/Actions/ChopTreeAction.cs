using System;
using UnityEngine;

public class ChopTreeAction : GoapAction
{
    private bool chopped = false;
    private TreeComponent targetTree;

    private float startTime = 0;
    public float workDuration = 2f;

    public ChopTreeAction()
    {
        addPrecondition("hasTool", true);
        addPrecondition("hasLogs", false);
        addEffect("hasLogs", true);
        cost = 1.0f;
    }

    public override void reset()
    {
        chopped = false;
        targetTree = null;
        startTime = 0;
    }

    public override bool isDone()
    {
        return chopped;
    }

    public override bool requiresInRange()
    {
        return true;
    }

    public override bool checkProceduralPrecondition(GameObject agent)
    {
        // OPTIMIZED: Modern Unity 6 API call
        TreeComponent[] trees = UnityEngine.Object.FindObjectsByType<TreeComponent>(FindObjectsSortMode.None);
        TreeComponent closest = null;
        float closestDist = Mathf.Infinity;

        foreach (TreeComponent tree in trees)
        {
            float dist = Vector3.Distance(tree.transform.position, agent.transform.position);
            if (dist < closestDist)
            {
                closest = tree;
                closestDist = dist;
            }
        }

        if (closest == null) return false;

        targetTree = closest;
        target = targetTree.gameObject;

        return targetTree != null;
    }

    public override bool perform(GameObject agent)
    {
        if (startTime == 0)
            startTime = Time.time;

        if (Time.time - startTime > workDuration)
        {
            BackpackComponent backpack = agent.GetComponent<BackpackComponent>();
            if (backpack != null)
            {
                backpack.numLogs += 1;

                if (backpack.tool != null)
                {
                    ToolComponent tool = backpack.tool.GetComponent<ToolComponent>();
                    if (tool != null)
                    {
                        tool.use(0.34f);
                        if (tool.destroyed())
                        {
                            Destroy(backpack.tool);
                            backpack.tool = null;
                        }
                    }
                }
            }
            chopped = true;
        }
        return true;
    }
}