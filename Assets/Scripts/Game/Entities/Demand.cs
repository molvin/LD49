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

            if (!Indicators.ContainsKey(need.Type))
            {
                continue;
            }

            var res = Indicators[need.Type];
            if (PressureLevels.ContainsKey(need.Type))
                res.SetValue(PressureLevels[need.Type]);
            else
                res.SetValue(0.0f);
            

        }


        GetComponent<Animator>().SetBool("Happy", IsSatisfied);

        if (IsSatisfied)
        {
            LastSatisfiedTime = Time.time;
            if (WarningSystem.Instance.ContainsWarning(Id))
                WarningSystem.Instance.CancelWarningObject(Id);
        }    
        else
        {

            float time_under_need = GetTimeUnderNeed();
            
            if(!WarningSystem.Instance.ContainsWarning(Id) && !GameManager.IsGameOver && GetComponentInParent<FactoryManager>())  
            {
                WarningSystem.Instance.CreateWarningObject(gameObject, Id, TimeToDestroy - time_under_need);
            }
           
            if (time_under_need > TimeToDestroy)
            {
                GameManager.Instance.GameOver();
            }
            
        }

    }

    internal void TransferConnectionsFrom(Demand doner_demand)
    {
        Debug.Log("Move connections to this");
    
        Edges = new List<Edge>();
        Edge edge = new Edge { Self = this, SelfSocket = 0, Other = null, OtherSocket = -1 };

        for (int i = 0; i < InteractionPoints.Count; i++)
        {
            edge.SelfSocket = (InteractionPoints[i].GetComponent<InteractionPoint>()).Socket;
            Edges.Add(edge);
        }

        var edges = doner_demand.Edges;
        for(int i = 0; i < edges.Count; i++)
        {
            edge = edges[i];
            edge.Self = this;
            Edges[edge.SelfSocket] = edge;
            Edge otherEdge = edge.Other.Edges[edge.OtherSocket];
            otherEdge.Other = this;
            otherEdge.Self.Edges[otherEdge.SelfSocket] = otherEdge;
            doner_demand.Edges.Clear();
            if(Edges[edge.SelfSocket].Other)
            {
                Hose hose = Edges[i].Other as Hose;
                if (Edges[edge.SelfSocket].OtherSocket == 0)
                    hose.Socket0.position = InteractionPoints[edge.SelfSocket].transform.position;
                if (Edges[edge.SelfSocket].OtherSocket == 1)
                    hose.Socket1.position = InteractionPoints[edge.SelfSocket].transform.position;
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
        if(Edges == null)
        {
            Edges = new List<Edge>();
            Edge edge = new Edge { Self = this, SelfSocket = 0, Other = null, OtherSocket = -1 };

            for (int i = 0; i < InteractionPoints.Count; i++)
            {
                edge.SelfSocket = (InteractionPoints[i].GetComponent<InteractionPoint>()).Socket;
                Edges.Add(edge);
            }
        }


        LastSatisfiedTime = Time.time;
        //DemandUI = GetComponentInChildren<ResourceWorldUI>();
    }

    private void OnEnable()
    {
        int j = 0;
        var indicators = GetComponentsInChildren<ResourceIndicator>(true);
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
