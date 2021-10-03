using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System;

public class Options : MonoBehaviour
{

    public AudioMixer mixer;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider SFXSlider;
    public Slider UISlider;


    void Start()
    {
        masterSlider.onValueChanged.AddListener(MasterSliderValueChanged);
        musicSlider.onValueChanged.AddListener(MusicSliderValueChanged);
        SFXSlider.onValueChanged.AddListener(SFXSliderValueChanged);
        UISlider.onValueChanged.AddListener(UISliderValueChanged);

        
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
        Debug.Log(actualValue);
        mixer.SetFloat(mixerName, actualValue);
       

    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
