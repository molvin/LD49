using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceIndicator : MonoBehaviour
{

    [Range(0,1)]
    public float demoDemand;
    [Range(0, 1)]
    public float demoValue;
    [Range(0, 100)]
    public float demoLeniency;

    public float animationTimeInSeconds = 0.3f;
    public float speed = 2;
    private float timer;
    

    private float demand = 0;
    private float targetValue = 0;
    private float currentValue = 0;

    private float valueAtStartOfAnim = 0;

    public SpriteRenderer demandMarker;
    public SpriteRenderer valueIndicator;

    public SpriteRenderer mirrorDemandMarker;
    public SpriteRenderer mirrorValueIndicator;
    public SpriteRenderer[] backGroundSprites;


    private float maxValue = 100;
    private float leniency;

    public ResourceType ResourceType;

    private void OnValidate()
    {
        maxValue = 100;
        SetDemand(demoDemand * 100, demoLeniency, ResourceType);

        SetVisualCurrentValue(demoValue);
    }

    private void Update()
    {
        if (timer < animationTimeInSeconds)
        {
            timer += Time.deltaTime;
            SetVisualCurrentValue(Mathf.Lerp(currentValue, targetValue, timer * speed) / 100);
            //SetVisualCurrentValue(targetValue / 100);

        }
    }

    public void SetDemand(float value, float leniency, ResourceType type)
    {
        if (value < 0 || value > maxValue)
        {
            throw new System.Exception("0-"+ maxValue+" please bitchboy! you gave me " + value);
        }
        demand = value;
        ResourceType = type;

        Vector3 localPos = demandMarker.transform.localPosition;
        demandMarker.transform.localPosition = new Vector3(localPos.x, (demand / maxValue) * 2 -1, localPos.z);

        Vector3 mirrorLocalPos = mirrorDemandMarker.transform.localPosition;
        mirrorDemandMarker.transform.localPosition = new Vector3(mirrorLocalPos.x, (demand / maxValue) * 2 - 1, mirrorLocalPos.z);

        foreach(var backgroundSprite in backGroundSprites)
        {
            var c = ResourceColor.TypeColor(type) * 0.85f;
            c.a = 1.0f;
            backgroundSprite.color = c;
        }
        demandMarker.color = mirrorDemandMarker.color = (type == ResourceType.Black ? Color.white : Color.black);
        this.leniency = leniency;
        Type = type;
    }

    public void SetValue(float value)
    {
        if (value < 0 || value > maxValue)
        {
            throw new System.Exception("0-" + maxValue + " please bitchboy! you gave me " + value);
        }
        valueAtStartOfAnim = currentValue;
        targetValue = value;
        timer = 0;
        
    }

    private ResourceType Type;

    private void SetVisualCurrentValue(float value)
    {
        currentValue = value * 100;
        valueIndicator.transform.localScale = new Vector3(1,
              value
               , 0);
        //valueIndicator.color = Mathf.Abs(value * 100f - demand) <= leniency ? Color.green : Color.red;
        valueIndicator.color = mirrorValueIndicator.color = ResourceColor.TypeColor(Type);
        mirrorValueIndicator.transform.localScale = new Vector3(1,
            value
             , 0);
      //  mirrorValueIndicator.color = Mathf.Abs(value * 100f - demand) <= leniency ? Color.green : Color.red;

    }



}
