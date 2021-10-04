using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolbarIcon : MonoBehaviour
{
    
    public Image icon;
    public Image outline;

    public Sprite splitter;
    public Sprite combinder;
    public Sprite valve;

    public void setHighlight(bool highlighted)
    {
        outline.enabled = highlighted;
    }
    public void SetIconByItem(Player.Item item)
    {
        if(Player.Item.Splitter == item)
        {
            icon.sprite = splitter;
        }
        else if (Player.Item.Combiner == item)
        {
            icon.sprite = combinder;
        }
        else if (Player.Item.Valve == item)
        {
            icon.sprite = valve;
        }
    }
}
