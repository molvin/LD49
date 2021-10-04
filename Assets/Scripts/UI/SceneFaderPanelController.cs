using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



[RequireComponent(typeof(Image))]
public class SceneFaderPanelController : MonoBehaviour
{
    public AnimationCurve animCurve;
    public float fadeTime = 0.8f;
    private Image fadeImage;
    private float fadeTimer = 0;
    private bool fadeIn = true;

    void Start()
    {
        this.fadeImage = this.GetComponent<Image>();

    }
    public void FadeIn(bool fade)
    {
        Debug.Log("FAAAADE");
        fadeIn = fade;
    }
    
    void Update()
    {
        fadeTimer += fadeIn ? Time.deltaTime : -Time.deltaTime;
        fadeTimer = Mathf.Clamp(fadeTimer, 0, fadeTime);
        Debug.Log(animCurve.Evaluate(fadeTimer / fadeTime));
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, animCurve.Evaluate(fadeTimer / fadeTime));
    }
}
