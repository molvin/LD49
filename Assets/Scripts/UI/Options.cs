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
        mixer.SetFloat("UI", arg0);
    }

    private void SFXSliderValueChanged(float arg0)
    {
        mixer.SetFloat("SFX", arg0);
    }

    private void MusicSliderValueChanged(float arg0)
    {
        mixer.SetFloat("Music", arg0);
    }

    private void MasterSliderValueChanged(float arg0)
    {
        mixer.SetFloat("Master", arg0);
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }
}
