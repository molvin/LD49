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
    public float animationSpeed = 0.3f;
    private float timer;
    

    private float demand = 0;
    private float targetValue = 0;
    private float currentValue = 0;

    private float valueAtStartOfAnim = 0;

    public SpriteRenderer demandMarker;
    public SpriteRenderer valueIndicator;
    public SpriteRenderer mirrorDemandMarker;
    public SpriteRenderer mirrorValueIndicator;

    private float maxValue = 100;
    private float leniency;

    public float TargetValue => targetValue;

    private void OnValidate()
    {
        maxValue = 100;
        SetDemand(demoDemand * 100, demoLeniency);

        SetVisualCurrentValue(demoValue);
    }

    private void Update()
    {
        currentValue = Mathf.Lerp(currentValue, targetValue, animationSpeed);
        SetVisualCurrentValue(currentValue / 100);
        if (timer < animationTimeInSeconds)
        {
            timer += Time.deltaTime;

        }
    }

    public void SetDemand(float value, float leniency)
    {
        if (value < 0 || value > maxValue)
        {
            throw new System.Exception("0-"+ maxValue+" please bitchboy! you gave me " + value);
        }
        demand = value;

        Vector3 localPos = demandMarker.transform.localPosition;
        Vector3 mirrorLocalPos = mirrorDemandMarker.transform.localPosition;
        demandMarker.transform.localPosition = new Vector3(localPos.x, (demand / maxValue) * 2 -1, localPos.z);
        mirrorDemandMarker.transform.localPosition = new Vector3(mirrorLocalPos.x, (demand / maxValue) * 2 - 1, mirrorLocalPos.z);
        this.leniency = leniency;
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

    private void SetVisualCurrentValue(float value)
    {
        valueIndicator.transform.localScale = new Vector3(1,
              value
               , 0);
        valueIndicator.color = Mathf.Abs(value * 100f - demand) <= leniency ? Color.green : Color.red;

        mirrorValueIndicator.transform.localScale = new Vector3(1,
              value
               , 0);
        mirrorValueIndicator.color = Mathf.Abs(value * 100f - demand) <= leniency ? Color.green : Color.red;

    }



}
