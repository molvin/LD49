using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceWorldUI : MonoBehaviour
{

    public Image wantedValueImage;
    public Image currentValueImage;
    [Range(0,100)]
    public float m_Value;
    private float realValue;
    [Range(0, 100)]
    public float demand;
    public float animTime;
    private float time;
    

    // Start is called before the first frame update
    void Start()
    {
        currentValueImage.fillAmount = 0;
    }


    private float fakeTimer;
    // Update is called once per frame
    void Update()
    {
        if(time < animTime)
        {
            time += Time.deltaTime;
            currentValueImage.fillAmount = Mathf.Lerp(realValue, m_Value, time / animTime) / 100;
        }
   
    }

    public void SetDemand(float value)
    {
        if (value < 0 || value > 100)
        {
            throw new System.Exception("0-100 please bitchboy!");
        }
        demand = value;
        wantedValueImage.transform.GetComponent<RectTransform>().localPosition = new Vector2(0, ((demand / 100) * 2) - 1);

    }

    public void SetValue(float inValue)
    {
        if(m_Value < 0 || m_Value > 100)
        {
            throw new System.Exception("0-100 please bitchboy!");
        }
        realValue = currentValueImage.fillAmount * 100;
        m_Value = inValue;
        time = 0;


    }


}
