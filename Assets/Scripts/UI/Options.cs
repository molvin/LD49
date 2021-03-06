using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using System;

public class Options : MonoBehaviour
{

    public AudioMixer mixer;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider SFXSlider;
    public Slider UISlider;

    public Button closeButton;

    public AnimationCurve animCurve;
    public bool isHidden = true;
    public float animTime = 1.5f;
    private float timer;

    private Vector3 startPos;


    private Resolution[] resolutions;
    private FullScreenMode[] WindowModes;

    Resolution selecterRes;
    FullScreenMode selecterMode;

    void Start()
    {
        masterSlider.onValueChanged.AddListener(MasterSliderValueChanged);
        musicSlider.onValueChanged.AddListener(MusicSliderValueChanged);
        SFXSlider.onValueChanged.AddListener(SFXSliderValueChanged);
        UISlider.onValueChanged.AddListener(UISliderValueChanged);
        float masterStartValue;
        float musicStartValue;
        float SFXStartValue;
        float UISliderStartValue;
        mixer.GetFloat("UI", out UISliderStartValue);
        mixer.GetFloat("SFX", out SFXStartValue);
        mixer.GetFloat("Music", out musicStartValue);
        mixer.GetFloat("Master", out masterStartValue);
        UISlider.SetValueWithoutNotify(Mathf.Pow(10, UISliderStartValue / 20));
        SFXSlider.SetValueWithoutNotify(Mathf.Pow(10, SFXStartValue / 20));
        musicSlider.SetValueWithoutNotify(Mathf.Pow(10, musicStartValue / 20));
        masterSlider.SetValueWithoutNotify(Mathf.Pow(10, masterStartValue / 20));


        startPos = this.transform.position;
        timer = animTime;

    }
    private void Update()
    {
        Animate();
    }

    

    
    private void Animate()
    {
        timer += isHidden ? Time.deltaTime : -Time.deltaTime;
        timer = Mathf.Clamp(timer, 0, animTime);
        float posModi = animCurve.Evaluate(timer / animTime);
        this.transform.position = new Vector3(startPos.x + (1500 * posModi), startPos.y, startPos.z);
        if(!isHidden)
        {
            enableAllChildren(true);
        }
        if (posModi < 0.1 && isHidden)
        {
            enableAllChildren(false);
        }
    }

    private void enableAllChildren(bool enable)
    {
        masterSlider.gameObject.SetActive(enable);
        musicSlider.gameObject.SetActive(enable);
        SFXSlider.gameObject.SetActive(enable);
        UISlider.gameObject.SetActive(enable);
        closeButton.gameObject.SetActive(enable);


        masterSlider.gameObject.SetActive(enable);
        musicSlider.gameObject.SetActive(enable);
        SFXSlider.gameObject.SetActive(enable);
        UISlider.gameObject.SetActive(enable);
    }

    public void SetHidden(bool hidden)
    {
        this.isHidden = hidden;
        if(!this.isHidden && Input.GetJoystickNames().Length > 1)
        {
            FindObjectOfType<EventSystem>().SetSelectedGameObject(masterSlider.gameObject);

        }
    }

    private void UISliderValueChanged(float arg0)
    {
        setVolume("UI", arg0);
    }

    private void SFXSliderValueChanged(float arg0)
    {
        setVolume("SFX", arg0);
    }

    private void MusicSliderValueChanged(float arg0)
    {
        setVolume("Music", arg0);
    }

    private void MasterSliderValueChanged(float arg0)
    {
        setVolume("Master", arg0);
    }
    private void setVolume(string mixerName, float arg)
    {
        float actualValue = Mathf.Log10(arg) * 20;
        actualValue = Mathf.Clamp(actualValue, -80, 0);
        mixer.SetFloat(mixerName, actualValue);
    }

}
