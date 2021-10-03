using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

public enum ResourceType
{
    None,
    Red,
    Green,
    Blue,
}

public struct Edge
{
    public Entity Self;
    public Entity Other;
    public int SelfSocket;
    public int OtherSocket;

    public Edge Cleared()
    {
        Edge Edge = this;
        Edge.Other = null;
        Edge.OtherSocket = -1;
        return Edge;
    }
}

public class EntityManager 
{
    // ENTITY GRID SIZE :: 1
    public List<Entity> Entities;

    public EntityManager()
    {
        Reset();
    }

    public Entity Add(Entity Entity, Vector3 WorldLocation)
    {
        Entities.Add(GameObject.Instantiate(
            Entity,
            WorldLocation,
            Quaternion.identity));
        return Entities[Entities.Count - 1];
    }

    public void Tick()
    {
        Entities.ForEach(e => e?.Clear());

        List<Demand> Demands = Entities.Where(e => e != null && e is Demand).Select(e => e as Demand).ToList();
        List<Resource> Resources = Entities.Where(e => e is Resource).Select(e => e as Resource).ToList();

        void Recurr(Entity Entity, PressurisedEnitity Parent)
        {
            if (Entity == null)
            {
                return;
            }

            if (Entity is Demand Demand)
            {
                Demand.UpdatePressureLevel(Parent.Type, Parent.Pressure);
            }
            //else if (Entity is Combinator Combinator)
            else if (Entity is Resource){}
            else if (Entity is PressurisedEnitity Pressurised)
            {
                if ((Pressurised.Type != ResourceType.None && Pressurised.Type != Parent.Type) || Pressurised.Pressure >= Parent.Pressure)
                {
                    return;
                }
                Pressurised.Type = Parent.Type;
                Pressurised.Pressure = (Entity is Valve Valve) ? Mathf.Min(Parent.Pressure, Valve.Gate) : Parent.Pressure;
                Pressurised.Edges.ForEach(Edge => Recurr(Edge.Other, Pressurised));
            }
            else
            {
                Assert.IsTrue(false);
            }
        }

        foreach (var Resource in Resources)
        {
            Resource.Edges.ForEach(Edge => Recurr(Edge.Other, Resource));
        }

        Entities.ForEach(e => e?.Tick());
    }


    public void Reset()
    {
        if (Entities != null)
        {
            foreach (var Entity in Entities)
            {
                GameObject.Destroy(Entity.gameObject);
            }
        }
        Entities = new List<Entity>();
        Entities.AddRange(GameObject.FindObjectsOfType<Entity>());
    }

    public void Destroy(Entity entity)
    {
        for(int i = 0; i < entity.Edges.Count; i++)
        {
            Edge edge = entity.Edges[i];
            if (edge.Other == null) 
                continue;
            Entity hose = edge.Other;
            int hoseSocket = edge.OtherSocket;
            hose.Edges[hoseSocket] = hose.Edges[hoseSocket].Cleared();
            foreach (var point in hose.InteractionPoints)
                point.SetActive(true);
        }

        Entities.Remove(entity);
        GameObject.Destroy(entity.gameObject);
    }
}
