using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : PressurisedEnitity
{
    public override bool CanConnect(Edge TryEdge, Edge IncommingEdge)
    {
        return TryEdge.Other == null && IncommingEdge.Self != TryEdge.Self;
    }

    public override void Clear() {}

    void Awake()
    {
        Edges = new List<Edge>{new Edge{ Self = this, SelfSocket = 0, Other = null, OtherSocket = -1 } };
    }
    public override string GetDebugInfo()
    {
        string Info = $"Type: {Type}, Current Output: {Pressure}\n";
        Info += $"Connedted: {Edges[0].Other}";
        return Info;
    }
}
