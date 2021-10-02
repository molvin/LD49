using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugGameManager : MonoBehaviour
{
    [System.Serializable]
    public struct DebugResourceInput
    {
        public KeyCode Button;
        public Resource Resource;
    }

    [System.Serializable]
    public struct DebugDemandInput
    {
        public KeyCode Button;
        public Demand Demand;
    }

    [Header("DEBUG")]    
    [SerializeField]
    public List<DebugResourceInput> Resources;
    [SerializeField]
    public List<DebugDemandInput> Demands;

    [Header("Grid")]    
    public Grid Grid;
    public int SizeX, SizeY;


    public void Awake()
    {
        Reset();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reset();
        }

        var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var Plane = new Plane(Vector3.up, Vector3.zero);
        float Dist;
        if (Plane.Raycast(Ray, out Dist))
        {
            Vector3 HitPoint = Ray.origin + Dist * Ray.direction;
            foreach (var Res in Resources)
            {
                if (!Res.Resource) continue;
                if (Input.GetKeyDown(Res.Button))
                {
                    Debug.Log("Hit: " + HitPoint);
                    Grid.TryAdd(Res.Resource, HitPoint);
                }
            }
        }
    }

    public void Reset()
    {
        if (Grid != null) Grid.Reset();
        Grid = new Grid(SizeX, SizeY);
    }
}
