using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class PauseMenu : MonoBehaviour
{
    public static bool Paused;
    public GameObject Parent;
    public Button resumeButton;
    public Button optionsButton;
    public Button exitButton;


    public void Start()
    {
        Parent.SetActive(false);
        resumeButton.onClick.AddListener(unPause);
        exitButton.onClick.AddListener(backToMainMenu);

    }

    private void Update()
    {
        if(Input.GetButtonDown("Start") || Input.GetKeyDown(KeyCode.Space))
        {
            Paused = !Paused;
            Time.timeScale = Paused ? 0.0f : 1.0f;
            Parent.SetActive(Paused);
        }
    }

    private void unPause()
    {
        Paused = false;
        Time.timeScale = Paused ? 0.0f : 1.0f;
        Parent.SetActive(Paused);
    }

    private void backToMainMenu()
    {
        Paused = false;
        Time.timeScale = Paused ? 0.0f : 1.0f;
        Parent.SetActive(Paused);
        SceneManager.LoadScene(0);

    }
}
