using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class AppHelper
{

    public static void Quit()
    {
        Debug.Log("please asdasd");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
            Debug.Log("please stop");
        #else
            Application.Quit();
            Debug.Log("please quit");
        #endif
    }
}
