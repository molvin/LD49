using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoPopupManager : MonoBehaviour
{

    private string infoPopupPrefabPath = "prefabs/Infopopup";


    public void AddInfo(string title, string body, Sprite sprite)
    {
        GameObject obj = (GameObject)Instantiate(Resources.Load(infoPopupPrefabPath), this.transform);
        obj.GetComponent<InfoPopup>().Init(title, body, sprite);
    }


}
