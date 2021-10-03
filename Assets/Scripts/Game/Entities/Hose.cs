using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hose : PressurisedEnitity
{
    public Transform Socket0;
    public Transform Socket1;

    public void UpdateSocketPositions(Vector3 start, Vector3 end)
    {
        Socket0.position = start;
        Socket1.position = end;
    }

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

    public override int GetEdgeIndex(Vector3 WorldPosition)
    {
        float dist0 = (WorldPosition - Socket0.position).sqrMagnitude;
        float dist1 = (WorldPosition - Socket1.position).sqrMagnitude;
        return dist0 < dist1 ? 0 : 1;
    }

    void Awake()
    {
        Edges = new List<Edge>(2);
        Edge Edge = new Edge { Self = this, SelfSocket = 0, Other = null, OtherSocket = -1 };
        for (int i = 0; i < 2; i++)
        {
            Edge.SelfSocket = i;
            Edges.Add(Edge);
        }
    }
}
