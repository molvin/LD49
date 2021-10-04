using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    public static bool Paused;
    public GameObject Parent;
    public Button resumeButton;
    public Button optionsButton;
    public Button exitButton;
    public Button skipTutorialButton;


    public void Start()
    {
        Parent.SetActive(false);
        resumeButton.onClick.AddListener(unPause);
        exitButton.onClick.AddListener(backToMainMenu);
        skipTutorialButton.onClick.AddListener(SkipTutorial);
        skipTutorialButton.enabled = GameManager.Instance.GetType() == typeof(TutorialGameManager);

    }

    private void Update()
    {
        if(Input.GetButtonDown("Start"))
        {
            Paused = !Paused;
            Parent.SetActive(Paused);
            if(Paused)
            {
                FindObjectOfType<EventSystem>().SetSelectedGameObject(resumeButton.gameObject);
            }
            Time.timeScale = Paused ? 0.0f : 1.0f;
        }
    }
    private void SkipTutorial()
    {
        PlayerPrefs.SetInt("playedTutorial", 1);
        unPause();
        SceneManager.LoadScene(0);
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
