using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Button StartButton;

    private void Start()
    {
        StartButton.onClick.AddListener(StartGame);
    }

    private void StartGame()
    {
        //TODO: Transition better, fades n shit
        SceneManager.LoadScene(1);
    }

}
