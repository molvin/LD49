using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningSystem : MonoBehaviour
{
    string prefabPath = "Prefabs/warning";
    Dictionary<int, GameObject> warnings = new Dictionary<int, GameObject>();

    public static WarningSystem Instance;

    public GameObject testItem;

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
        GameObject obj = (GameObject)Instantiate(Resources.Load(prefabPath));
        obj.GetComponent<WarningObject>().constructor(target.transform, time);
        warnings.Add(id, target);
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
