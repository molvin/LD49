using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPopup : MonoBehaviour
{

    public Text infoBody;
    public Text infoTitle;
    // Start is called before the first frame update
    public void Init(string title, string body)
    {
        infoTitle.text = title;
        infoBody.text = body;
    }
    public void Update()
    {
        if (Input.GetButtonDown("Interact"))
        {
            Destroy(this.gameObject);
        }

    }
}
