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

public class Grid 
{
    // ENTITY GRID SIZE :: 1
    private int SizeX, SizeY;

    public List<Entity> Entities;

    public Grid(int SizeX, int SizeY)
    {
        this.SizeX = SizeX;
        this.SizeY = SizeY;

        Reset();
    }

    public Vector2Int WorldToGrid(Vector3 Position)
    {
        Position.x += SizeX * 0.5f;
        Position.y += SizeY * 0.5f;
        return new Vector2Int((int)Position.x, (int)Position.y);
    }

    public Vector3 GridToWorld(Vector2Int Position)
    {
        return new Vector3(
            Position.x + 0.5f - SizeX * 0.5f,
            Position.y + 0.5f - SizeY * 0.5f,
            0.0f
        );
    }

    public bool TryAdd(Entity Entity, Vector3 WorldLocation)
    {
        Vector2Int GridPosition = WorldToGrid(WorldLocation);
        if (GridPosition.x >= 0 && GridPosition.x < SizeX
            && GridPosition.y >= 0 && GridPosition.y < SizeY)
        {
            int Position = GridPosition.x + GridPosition.y * SizeX;
            if (!Entities[Position])
            {
                Entities[Position] = GameObject.Instantiate(
                    Entity,
                    GridToWorld(GridPosition),
                    Quaternion.identity);
                return true;
            }
        }
        return false;
    }

    public bool TryGet(out Entity Entity, Vector2Int GridPosition)
    {
        Entity = null;
        if (GridPosition.x >= 0 && GridPosition.x < SizeX
            && GridPosition.y >= 0 && GridPosition.y < SizeY)
        {
            int Position = GridPosition.x + GridPosition.y * SizeX;
            if (Entities[Position])
            {
                Entity = Entities[Position];
                return true;
            }
        }
        return false;
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
                if (Entity)
                {
                    GameObject.Destroy(Entity.gameObject);
                }
            }
        }
        Entities = new List<Entity>(new Entity[SizeX * SizeY]);
    }
}
