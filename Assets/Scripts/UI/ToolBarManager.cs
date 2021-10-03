using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolBarManager : MonoBehaviour
{

    string toolbarPath = "prefabs/toolbar";
    string iconPath = "prefabs/toolbarIcon";
    public AnimationCurve hideAnimCurve;
    private bool isHidden = false;
    public float animTime = 1f;
    private float animTimer;

    private Vector3 startPos;
    Player player;

    GameObject toolbarObj;

    Dictionary<Player.Item, GameObject> items = new Dictionary<Player.Item, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        animTimer = animTime;        

        toolbarObj = (GameObject)Instantiate(Resources.Load(toolbarPath), this.transform);
        startPos = toolbarObj.transform.position;

        player = FindObjectOfType<Player>();
        GenerateItems(player.getItems());
        
    }
    //dummyinput
    private void Update()
    {


      

        doAnim();
        updatePlayerGui();

    }

    private void updatePlayerGui()
    {
        if(player.CurrentState == Player.State.Build)
        {
            SetToolbarHidden(false);
            SetItemActive((Player.Item)player.SelectedItem);
        } else
        {
            SetToolbarHidden(true);
            DisableAllItems();
        }

    }

    void GenerateItems(List<Player.Item> items)
    {
        foreach(Player.Item item in items)
        {
            AddIcon(item, null);

        }
    }

    void doAnim()
    {
        animTimer += isHidden ? Time.deltaTime : -Time.deltaTime;
        animTimer = Mathf.Clamp(animTimer, 0, animTime);
        float posModi = hideAnimCurve.Evaluate(animTimer / animTime);
        toolbarObj.transform.position = new Vector3(startPos.x, startPos.y + (200 * posModi), startPos.z);
    }

    public void SetToolbarHidden(bool hidden)
    {
        isHidden = hidden;
    }

    public void DisableAllItems()
    {
        foreach (GameObject item in items.Values)
        {
            item.GetComponentInChildren<Outline>().enabled = false;
        }
    }

    public void SetItemActive(Player.Item id)
    {
        DisableAllItems();
        items[id].GetComponentInChildren<Outline>().enabled = true;
        
    }

    void AddIcon(Player.Item id, Sprite icon)
    {
        GameObject iconobj = (GameObject)Instantiate(Resources.Load(iconPath), toolbarObj.transform);
        
        //iconobj.GetComponentInChildren<Image>().sprite = icon;
        iconobj.GetComponentInChildren<Text>().text = "0";
        items.Add(id,iconobj);
    }

    public void SetIconCount(Player.Item id, int count)
    {
        items[id].GetComponentInChildren<Text>().text = count.ToString();
    }

}



