using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool Paused;
    public GameObject Parent;

    public void Start()
    {
        Parent.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetButtonDown("Start"))
        {
            Paused = !Paused;
            Time.timeScale = Paused ? 0.0f : 1.0f;
            Parent.SetActive(Paused);
        }
    }
}
