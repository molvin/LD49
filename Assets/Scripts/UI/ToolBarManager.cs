using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolBarManager : MonoBehaviour
{
    
    string bulldowsePath = "prefabs/Bulldowse";
    public AnimationCurve bulldowserAnimCurve;
    GameObject bulldowseObjectbarObj;
    private bool bulldowseIsHidden = false;
    public float bulldowseAnimTime = 1f;
    private float bulldowseAnimTimer;
    private Vector3 bulldowsStartPos;

    string toolbarPath = "prefabs/toolbar";
    string iconPath = "prefabs/toolbarIcon";
    public AnimationCurve hideAnimCurve;
    private bool isHidden = false;
    public float animTime = 1f;
    private float animTimer;

    private Vector3 startPos;
    Player player;

    GameObject toolbarObj;

    Dictionary<Player.Item, ToolbarIcon> items = new Dictionary<Player.Item, ToolbarIcon>();

    // Start is called before the first frame update
    public void Init()
    {
        animTimer = animTime;
        bulldowseAnimTimer = bulldowseAnimTime;
        toolbarObj = (GameObject)Instantiate(Resources.Load(toolbarPath), this.transform);
        startPos = toolbarObj.transform.position;

        bulldowseObjectbarObj = (GameObject)Instantiate(Resources.Load(bulldowsePath), this.transform);
        bulldowsStartPos = bulldowseObjectbarObj.transform.position;


        player = GetComponentInParent<Player>();
        GenerateItems(player.getItems());
        
    }

    //dummyinput
    private void Update()
    {


      

        doAnim();
        updatePlayerGui();
        doBulldowseAnim();
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
        bulldowseIsHidden = player.CurrentState != Player.State.Destroy; 

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
    void doBulldowseAnim()
    {
        bulldowseAnimTimer += bulldowseIsHidden ? Time.deltaTime : -Time.deltaTime;
        bulldowseAnimTimer = Mathf.Clamp(bulldowseAnimTimer, 0, bulldowseAnimTime);
        float posModi = hideAnimCurve.Evaluate(bulldowseAnimTimer / bulldowseAnimTime);
        bulldowseObjectbarObj.transform.position = new Vector3(bulldowsStartPos.x, bulldowsStartPos.y - (200 * posModi), bulldowsStartPos.z);
    }

    public void SetToolbarHidden(bool hidden)
    {
        isHidden = hidden;
    }

    public void DisableAllItems()
    {
        foreach (ToolbarIcon item in items.Values)
        {
            item.setHighlight(false);
        }
    }

    public void SetItemActive(Player.Item id)
    {
        DisableAllItems();
        items[id].setHighlight(true);

    }

    void AddIcon(Player.Item id, Sprite icon)
    {
        GameObject iconobj = (GameObject)Instantiate(Resources.Load(iconPath), toolbarObj.transform);
        ToolbarIcon iconScript =  iconobj.GetComponent<ToolbarIcon>();
        iconScript.SetIconByItem(id);
        items.Add(id, iconScript);
    }

    public void SetIconCount(Player.Item id, int count)
    {
        items[id].GetComponentInChildren<Text>().text = count.ToString();
    }

}



