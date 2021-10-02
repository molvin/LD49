using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DebugGameManager : MonoBehaviour
{
    [System.Serializable]
    public struct DebugEntityInput
    {
        public KeyCode Button;
        public Entity Prefab;
    }

    [Header("DEBUG")]    
    [SerializeField]
    public List<DebugEntityInput> EntityEntries;

    [Header("Grid")]    
    public Grid Grid;
    public int SizeX, SizeY;

    private Entity SelectedThing;
    private int SelectedThingSocket;


    public void Awake()
    {
        Reset();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Reset();
        }

        var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var Plane = new Plane(Vector3.forward, Vector3.zero);
        float Dist;
        if (Plane.Raycast(Ray, out Dist))
        {
            Vector3 HitPoint = Ray.origin + Dist * Ray.direction;
            foreach (var Entry in EntityEntries)
            {
                if (!Entry.Prefab) continue;
                if (Input.GetKeyDown(Entry.Button))
                {
                    Debug.Log("Hit: " + HitPoint);
                    Grid.TryAdd(Entry.Prefab, HitPoint);
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                SelectThing(HitPoint);
            }
            if (Input.GetMouseButtonDown(1))
            {
                RemoveSelectedConnection();
            }
        }
        DrawConnections();


        Grid.Tick();
    }

    void DrawConnections()
    {
        List<LineRenderer> Renderers = GetComponentsInChildren<LineRenderer>().ToList();
        foreach (var Ren in Renderers)
        {
            Ren.positionCount = 0;
        }

        int RenI = 0;
        foreach (var Entity in Grid.Entities)
        {
            if (Entity)
            {
                foreach (var Edge in Entity.Edges)
                {
                    if (Edge.Self && Edge.Other)
                    {
                        if (RenI >= Renderers.Count)
                        {
                            var R = new GameObject().AddComponent<LineRenderer>();
                            R.transform.parent = transform;
                            R.SetWidth(0.2f, 0.2f);
                            Renderers.Add(R);
                        }

                        var Ren = Renderers[RenI++];
                        Ren.positionCount = 2;
                        Ren.SetPosition(0, Edge.Self.transform.position);
                        Ren.SetPosition(1, Edge.Other.transform.position);
                    }
                }
            }
        }
    }

    void SelectThing(Vector3 WorldPosition)
    {
        Entity Entity;
        if (Grid.TryGet(out Entity, Grid.WorldToGrid(WorldPosition)))
        {
            Debug.Log($"Selected {Entity.name}");

            int EntitySocket = Entity.GetEdgeIndex(WorldPosition);

            if (SelectedThing)
            {
                SelectedThing.TryConnect(SelectedThing.Edges[SelectedThingSocket], Entity.GetEdge(WorldPosition));
            }
            SelectedThing = Entity;
            SelectedThingSocket = EntitySocket;
        }
    }

    void RemoveSelectedConnection()
    {
        if (SelectedThing)
        {
            SelectedThing.Disconnect(SelectedThing.Edges[SelectedThingSocket]);
        }
        SelectedThing = null;
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        if (SelectedThing)
        {
            Vector3 Point = Camera.main.WorldToScreenPoint(SelectedThing.transform.position);
            var Rect = new Rect(Point.x - 50, Screen.height - Point.y - 50, 100.0f, 50.0f);
            GUI.Label(Rect, "SELECTED");
        }

        foreach (var Entity in Grid.Entities)
        {
            if (Entity)
            {
                string Info = Entity.GetDebugInfo();
                Vector3 Point = Camera.main.WorldToScreenPoint(Entity.transform.position);
                var Rect = new Rect(Point.x, Screen.height - Point.y, 250.0f, 80.0f);
                GUI.Box(Rect, Info);
            }
        }
    }
#endif

    public void Reset()
    {
        if (Grid != null) Grid.Reset();
        Grid = new Grid(SizeX, SizeY);

        List<LineRenderer> Renderers = GetComponentsInChildren<LineRenderer>().ToList();
        foreach (var Ren in Renderers)
        {
            Ren.positionCount = 0;
        }
    }
}
