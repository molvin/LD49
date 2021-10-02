using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningSystem : MonoBehaviour
{
    string prefabPath = "Prefabs/warning";
    Dictionary<string, GameObject> warnings = new Dictionary<string, GameObject>();


    public GameObject testItem;
    private void Start()
    {
        if(testItem != null)
        {
            CreateWarningObject(testItem, "test", 60);
        }
    }

    void CreateWarningObject(GameObject target, string id, int time)
    {
        GameObject obj = (GameObject)Instantiate(Resources.Load(prefabPath));
        obj.GetComponent<WarningObject>().constructor(target.transform, time);
        warnings.Add(id, target);
    }

    void CancelWarningObject(string id)
    {
        GameObject obj = warnings[id];
        Destroy(obj);
        warnings.Remove(id);
    }



}
