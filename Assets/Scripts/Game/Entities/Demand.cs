using System;
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
    public float NeedLeniency = 10;
    protected float LastSatisfiedTime;
    public bool IsSatisfied;
    public bool ShouldAddOnActivation = true;


    protected Dictionary<ResourceType, float> PressureLevels = new Dictionary<ResourceType, float>();
    protected Dictionary<ResourceType, ResourceIndicator> Indicators = new Dictionary<ResourceType, ResourceIndicator>();

    public override void Tick()
    {
        base.Tick();

        //If no time has changed don't care about the need
        if (Time.time == LastSatisfiedTime)
            return;

        IsSatisfied = true;
        foreach(Need need in Needs)
        {
            if(PressureLevels.ContainsKey(need.Type))
            {
                float pressure_value = PressureLevels[need.Type];
                if (Mathf.Abs(need.Value - pressure_value) >= NeedLeniency)
                    IsSatisfied = false;

            }
            else if (IsSatisfied)
            {
                IsSatisfied = false;
            }

            var res = Indicators[need.Type];
            Debug.Log("Setting");
            if (PressureLevels.ContainsKey(need.Type))
                res.SetValue(PressureLevels[need.Type]);
            else
                res.SetValue(0.0f);
            

        }



        if (IsSatisfied)
        {
            LastSatisfiedTime = Time.time;
            if (WarningSystem.Instance.ContainsWarning(gameObject.GetInstanceID()))
                WarningSystem.Instance.CancelWarningObject(gameObject.GetInstanceID());
        }    
        else
        {
            float time_under_need = GetTimeUnderNeed();

            if (time_under_need > TimeToDestroy)
            {
                GameManager.Instance.GameOver();
            }
            else if(TimeToDestroy - (TimeToDestroy - time_under_need) > WarningSystem.Instance.WarningTime && !WarningSystem.Instance.ContainsWarning(gameObject.GetInstanceID()))
            {
                WarningSystem.Instance.CreateWarningObject(gameObject, gameObject.GetInstanceID(), TimeToDestroy - time_under_need);
            }
           
        }

    }

    internal void TransferConnectionsTo(Demand doner_demand)
    {
        Debug.Log("Move connections to this");
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
        foreach (var res in Indicators.Values)
            res.SetValue(0.0f);
    }
    public override bool CanConnect(Edge TryEdge, Edge IncommingEdge)
    {
        return TryEdge.Other == null && IncommingEdge.Self != TryEdge.Self;
    }

    void Awake()
    {
        //Edges = new List<Edge>{new Edge{ Self = this, SelfSocket = 0, Other = null, OtherSocket = -1 } };
        Edges = new List<Edge>();
        Edge edge = new Edge { Self = this, SelfSocket = 0, Other = null, OtherSocket = -1 };
        for (int i = 0; i < InteractionPoints.Count; i++)
        {
            edge.SelfSocket = (InteractionPoints[i].GetComponent<InteractionPoint>()).Socket;
            Edges.Add(edge);
        }

        LastSatisfiedTime = Time.time;
        //DemandUI = GetComponentInChildren<ResourceWorldUI>();
    }

    private void OnEnable()
    {
        int j = 0;
        var indicators = GetComponentsInChildren<ResourceIndicator>();
        foreach (Need need in Needs)
        {
           // DemandUI.SetDemand(need.Value, NeedLeniency
            indicators[j].SetDemand(need.Value, NeedLeniency, need.Type);
            Indicators.Add(need.Type, indicators[j++]);
        }
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
