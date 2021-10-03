using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demand : Entity
{
    [System.Serializable]
    public struct Need
    {
        public ResourceType Type;
        public float Value;
    }

    public List<Need> Needs;
    public float TimeToDestroy;
    protected float TimeUnderNeed;

    protected Dictionary<ResourceType, float> PressureLevels = new Dictionary<ResourceType, float>();

    public void UpdatePressureLevel(ResourceType Type, float Pressure)
    {
        float CurrentLevel;
        if (PressureLevels.TryGetValue(Type, out CurrentLevel))
        {
            if (CurrentLevel < Pressure)
            {
                PressureLevels[Type] = Pressure;
            }
        }
        else
        {
            PressureLevels.Add(Type, Pressure);
        }
    }

    public override void Clear()
    {
        PressureLevels.Clear(); 
    }
    public override bool CanConnect(Edge TryEdge, Edge IncommingEdge)
    {
        return TryEdge.Other == null && IncommingEdge.Self != TryEdge.Self;
    }

    void Awake()
    {
        Edges = new List<Edge>{new Edge{ Self = this, SelfSocket = 0, Other = null, OtherSocket = -1 } };
    }
    private void Update()
    {
        print(GetDebugInfo());
    }

    public override string GetDebugInfo()
    {
        string Info = "";
        foreach (var Need in Needs)
        {
            Info += $"Need: {Need.Type}, Value: {Need.Value}, Pressure: {(PressureLevels.ContainsKey(Need.Type) ? PressureLevels[Need.Type] : 0.0f)}\n";
        }
        Info += $"Time to destruction: {(TimeUnderNeed <= 0.0f ? -1.0f : TimeToDestroy - TimeUnderNeed)}\n";
        Info += $"Connedted: {Edges[0].Other}";
        return Info;
    }
}
