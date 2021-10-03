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
    protected float LastSatisfiedTime;

    protected Dictionary<ResourceType, float> PressureLevels = new Dictionary<ResourceType, float>();

    public override void Tick()
    {
        base.Tick();

        //If no time has changed don't care about the need
        if (Time.time == LastSatisfiedTime)
            return;

        bool has_what_it_needs = true;
        foreach(Need need in Needs)
        {
            if(PressureLevels.ContainsKey(need.Type))
            {
                if(need.Value <= PressureLevels[need.Type])
                    continue;
                
            }

            has_what_it_needs = false;
            break;
        }

        if (has_what_it_needs)
        {
            LastSatisfiedTime = Time.time;
        }    
        else
        {
            float time_under_need = GetTimeUnderNeed();

            if (time_under_need > TimeToDestroy)
            {
                GameManager.Instance.GameOver();
            }
            else if(time_under_need / TimeToDestroy > 0.9 && !WarningSystem.Instance.ContainsWarning(gameObject.GetInstanceID()))
            {
                WarningSystem.Instance.CreateWarningObject(gameObject, gameObject.GetInstanceID(), TimeToDestroy - time_under_need);
            }
           
        }

    }

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
        LastSatisfiedTime = Time.time;
    }
    private void Update()
    {
       // print(GetDebugInfo());
    }

    public override string GetDebugInfo()
    {
        string Info = "";
        foreach (var Need in Needs)
        {
            Info += $"Need: {Need.Type}, Value: {Need.Value}, Pressure: {(PressureLevels.ContainsKey(Need.Type) ? PressureLevels[Need.Type] : 0.0f)}\n";
        }
        Info += $"Time to destruction: {(GetTimeUnderNeed() <= 0.0f ? -1.0f : TimeToDestroy - GetTimeUnderNeed())}\n";
        Info += $"Connedted: {Edges[0].Other}";
        return Info;
    }

    private float GetTimeUnderNeed()
    {
        return Time.time - LastSatisfiedTime;
    }
}
