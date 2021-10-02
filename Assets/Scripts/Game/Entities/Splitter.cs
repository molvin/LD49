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
                return Type == ResourceType.None || Press.Type == Type;
            }
        }
        return false;
    }
    
    public override int GetEdgeIndex(Vector3 WorldPosition)
    {
        Vector3 Delta = WorldPosition - transform.position;
        if (Mathf.Abs(Delta.x) > Mathf.Abs(Delta.y))
        {
            if (Delta.x > 0)
            {
                return 0;
            }
            else
            {
                return 2;
            }
        }
        else
        {
            if (Delta.y > 0)
            {
                return 3;
            }
            else
            {
                return 1;
            }
        }
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
