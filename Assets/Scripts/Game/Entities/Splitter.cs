using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splitter : PressurisedEnitity
{
    public override bool CanConnect(Edge TryEdge, Edge IncommingEdge)
    {
        if (TryEdge.Other == null && IncommingEdge.Self != TryEdge.Self)
        {
            if (IncommingEdge.Self is Demand)
            {
                return true;
            }
            if (IncommingEdge.Self is PressurisedEnitity Press)
            {
                return Type == ResourceType.None || Press.Type == ResourceType.None || Press.Type == Type;
            }
        }
        return false;
    }
    
    void Awake()
    {
        Edges = new List<Edge>(4);
        Edge Edge = new Edge{ Self = this, SelfSocket = 0, Other = null, OtherSocket = -1 };
        for (int i = 0; i < 4; i++)
        {
            Edge.SelfSocket = i;
            Edges.Add(Edge);
        }
    }
}
