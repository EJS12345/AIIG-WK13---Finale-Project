using UnityEngine;
using System.Collections.Generic;

public class Villager : Labourer
{

    public override HashSet<KeyValuePair<string, object>> createGoalState()
    {
        HashSet<KeyValuePair<string, object>> goals = new HashSet<KeyValuePair<string, object>>();

        if (hunger >= 50f)
        {
            
            goals.Add(new KeyValuePair<string, object>("isFed", true));
        }
        else
        {
            
            goals.Add(new KeyValuePair<string, object>("collectLogs", true));
        }

        return goals;
    }
}