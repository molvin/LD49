using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialGameManager : GameManager
{

    [Header("Interval")]
    public Demand[] demandsToCompleate;
    private int lastTutorialScene = 7;
    public SceneFaderPanelController fader;
    public InfoPopupManager infoPopupManager;

    public string infoMessageTitle;
    [TextArea]
    public string infoMessageBody;
    public Sprite inforMessageImage;
    private bool loadingNextScene = false;
    protected void Start()
    {
        if(fader == null)
        {
            fader = FindObjectOfType<SceneFaderPanelController>();
        }
        if (infoPopupManager == null)
        {
            infoPopupManager = FindObjectOfType<InfoPopupManager>();
        }
        infoPopupManager.AddInfo(infoMessageTitle, infoMessageBody, inforMessageImage);
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
        if (loadingNextScene)
            return;

        loadingNextScene = true;

        StartCoroutine(NextLevelSoon());

      
    }
    private IEnumerator NextLevelSoon()
    {
        yield return new WaitForSeconds(0.5f);
        fader.FadeIn(false);
        yield return new WaitForSeconds(1.5f);
        Scene currentScene = SceneManager.GetActiveScene();
        int nextScene = currentScene.buildIndex + 1;
        if (nextScene > lastTutorialScene)
        {
            PlayerPrefs.SetInt("playedTutorial", 1);
            nextScene = 0;
        }
        SceneManager.LoadScene(nextScene);
    }

}
