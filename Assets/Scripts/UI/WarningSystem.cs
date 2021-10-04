using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningSystem : MonoBehaviour
{
    string prefabPath = "Prefabs/UIWarning";
    Dictionary<int, GameObject> warnings = new Dictionary<int, GameObject>();

    public static WarningSystem Instance;

    public GameObject testItem;
    public Transform parentCanvas;
    public float WarningTime = 10f;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if(testItem != null)
        {
            CreateWarningObject(testItem, -1, 60);
        }
    }

    public void ClearAll()
    {
        foreach(var go in warnings.Values)
        {
            Destroy(go);
        }
        warnings.Clear();
    }

    public void CreateWarningObject(GameObject target, int id, float time)
    {
        Debug.Log("CREATEING " + target);
        GameObject obj = (GameObject)Instantiate(Resources.Load(prefabPath), parentCanvas);
        obj.GetComponent<WarningObject>().constructor(target.transform, time);
        warnings.Add(id, obj);
    }

    public void CancelWarningObject(int id)
    {
        GameObject obj = warnings[id];
        Destroy(obj);
        warnings.Remove(id);
    }

    internal bool ContainsWarning(int id)
    {
        return warnings.ContainsKey(id);
    }
}
