using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Button StartButton;
    public Button ExitButton;
    public Button OptionsButton;

    public AnimationCurve fadeOutCurve;
    public float fadeOutTime = 2;
    public Image fadePanel;
    private void Start()
    {
        StartButton.onClick.AddListener(StartGame);
        ExitButton.onClick.AddListener(ExitGame);
        

    }

    private void StartGame()
    {
        //TODO: Transition better, fades n shit
        StartAfterDelay();
    }

    private void ExitGame()
    {
        AppHelper.Quit();
    }


    private void StartAfterDelay()
    {
        StartCoroutine("StartAfterSeconds", fadeOutTime);
    }
    IEnumerator StartAfterSeconds(float seconds)
    {
        float startTime = Time.deltaTime;
        float timer = 0;
        while(seconds > timer)
        {
            timer += Time.deltaTime;
            float factor = fadeOutCurve.Evaluate(timer/seconds);
            fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b,factor);
            yield return null;
        }
        
        SceneManager.LoadScene(1);
    }

}
