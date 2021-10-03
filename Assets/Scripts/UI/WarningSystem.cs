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

    public void CreateWarningObject(GameObject target, int id, float time)
    {
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
