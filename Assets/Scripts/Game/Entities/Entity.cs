using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public List<Edge> Edges;
    public List<GameObject> InteractionPoints;

    public virtual void Tick() { }

    public virtual void Clear() {}
    public int GetEdgeIndex(Vector3 WorldPosition)
    {
        float distance = 10000000000.0f;
        InteractionPoint closest = null;
        foreach (var ip in InteractionPoints)
        {
            float d = (WorldPosition - ip.transform.position).magnitude;
            var i = ip.GetComponent<InteractionPoint>();
            if (d < distance && (i.InteractType == InteractionPoint.Interactable.Socket || i.InteractType == InteractionPoint.Interactable.Hose))
            {
                distance = d;
                closest = i;
            }
        }
        return closest.Socket;
    }
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
