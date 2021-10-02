using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum ResourceType
{
    None,
    Water,
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
        Position.z += SizeY * 0.5f;
        return new Vector2Int((int)Position.x, (int)Position.z);
    }

    public Vector3 GridToWorld(Vector2Int Position)
    {
        return new Vector3(
            Position.x + 0.5f - SizeX * 0.5f,
            0.0f,
            Position.y + 0.5f - SizeY * 0.5f
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

    public void Tick()
    {

    }
    public void Reset()
    {
        Test();
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

    public void Test()
    {
        Vector3 v = new Vector3(13.2f, 12.2f, 3.0f);
        Vector3 v2 = GridToWorld(WorldToGrid(v));
        Assert.IsTrue(v2 == new Vector3(13.5f, 0.0f, 3.5f));
    }
}
