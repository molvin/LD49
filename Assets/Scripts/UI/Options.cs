using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Events;
using System;

public class Options : MonoBehaviour
{

    public AudioMixer mixer;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider SFXSlider;
    public Slider UISlider;



    public AnimationCurve animCurve;
    public bool isHidden = true;
    public float animTime = 1.5f;
    private float timer;

    private Vector3 startPos;


    private Resolution[] resolutions;
    private FullScreenMode[] WindowModes;
    public Dropdown resDropDown;
    public Dropdown modeDropDown;

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




        SetupResolutionDropDown();
        SetupWindowModeDropDown();
    }
    private void Update()
    {
        Animate();
    }

    private void SetupWindowModeDropDown()
    {
        WindowModes = ((FullScreenMode[])System.Enum.GetValues(typeof(FullScreenMode)));
        List<String> options = new List<string>();
        foreach (FullScreenMode mode in this.WindowModes)
        {
            options.Add(mode.ToString());
        }
        modeDropDown.ClearOptions();
        modeDropDown.AddOptions(options);
        modeDropDown.SetValueWithoutNotify((int)Screen.fullScreenMode);
        
        modeDropDown.onValueChanged.AddListener(delegate
        {
            SetWindowMode();
        });

    }
    private void SetWindowMode()
    {
        selecterMode = this.WindowModes[modeDropDown.value];
        Screen.SetResolution(selecterRes.width, selecterRes.height, selecterMode, selecterRes.refreshRate);

    }
    private void SetupResolutionDropDown()
    {
        this.resolutions = Screen.resolutions;
        List<String> options = new List<string>();
        foreach (Resolution res in this.resolutions)
        {

            options.Add(res.ToString());
        }
        resDropDown.ClearOptions();
        resDropDown.AddOptions(options);
        int index = options.FindIndex(a => a == Screen.currentResolution.ToString());
        resDropDown.SetValueWithoutNotify(index);

        resDropDown.onValueChanged.AddListener(delegate
        {
            SetResulution();
        });
    }
   

    private void SetResulution()
    {
        selecterRes = this.resolutions[resDropDown.value];
        Screen.SetResolution(selecterRes.width,selecterRes.height, selecterMode, selecterRes.refreshRate);
    }

    private void Animate()
    {
        timer += isHidden ? Time.deltaTime : -Time.deltaTime;
        timer = Mathf.Clamp(timer, 0, animTime);
        float posModi = animCurve.Evaluate(timer / animTime);
        this.transform.position = new Vector3(startPos.x + (1500 * posModi), startPos.y, startPos.z);
    }

    public void SetHidden(bool hidden)
    {
        this.isHidden = hidden;
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
        Debug.Log(actualValue);
        mixer.SetFloat(mixerName, actualValue);
       

    }

}
