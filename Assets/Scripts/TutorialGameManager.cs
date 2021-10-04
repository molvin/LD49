using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialGameManager : GameManager
{

    [Header("Interval")]
    public Demand[] demandsToCompleate;

    protected void Start()
    {
         
    }

    private void Update()
    {
        m_EntityManager.Tick();
        AreAllDemandsSatisfied();
    }

    private void AreAllDemandsSatisfied()
    {
        bool notSatified = false;
        foreach(Demand demand in demandsToCompleate)
        {
            notSatified = !demand.IsSatisfied ? true : notSatified;
        }
        if(!notSatified)
        {
            NextLevel();
        }
    }

    private void NextLevel()
    {
        Debug.Log("We done");

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex + 1);
    }
}
