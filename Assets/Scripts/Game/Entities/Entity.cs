using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public List<Edge> Edges;

    public virtual void Tick() { }

    public virtual void Clear() {}
    public virtual int GetEdgeIndex(Vector3 WorldPosition) => 0;
    public Edge GetEdge(Vector3 WorldPosition) => Edges[GetEdgeIndex(WorldPosition)];
    public void SetEdge(int index, Edge edge) => Edges[index] = edge;

    public bool TryConnect(Edge TryEdge, Edge IncommingEdge)
    {
        bool a = CanConnect(TryEdge, IncommingEdge); 
        bool b = IncommingEdge.Self.CanConnect(IncommingEdge, TryEdge);
        if (a && b)
        {
            Connect(TryEdge, IncommingEdge);
            IncommingEdge.Self.Connect(IncommingEdge, TryEdge);
            return true;
        }
        return false;
    }

    public void Connect(Edge TryEdge, Edge IncommingEdge)
    {
        TryEdge.Other = IncommingEdge.Self;
        TryEdge.OtherSocket = IncommingEdge.SelfSocket;
        Edges[TryEdge.SelfSocket] = TryEdge;
    }
    public abstract bool CanConnect(Edge TryEdge, Edge IncommingEdge);
    public void Disconnect(Edge Edge)
    {
        Edges[Edge.SelfSocket] = Edge.Cleared();
        if (Edge.Other)
        {
            Edge.Other.Edges[Edge.OtherSocket] = Edge.Other.Edges[Edge.OtherSocket].Cleared();
        }
    }
    public virtual string GetDebugInfo() => "";
}
