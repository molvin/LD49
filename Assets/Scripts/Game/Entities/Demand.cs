using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Demand : Entity
{
    public struct Need
    {
        ResourceType Type;
        float Value;
    }

    public List<Need> Needs;
    public float TimeToDestroy;
    protected float TimeUnderNeed;
}
