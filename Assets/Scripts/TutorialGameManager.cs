using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialGameManager : GameManager
{

    [Header("Interval")]
    public Demand[] demandsToCompleate;

    protected void Start()
    {
        
        foreach (Demand demand in demandsToCompleate)
        {
            m_EntityManager.Add(demand, demand.transform.position);
        }
         
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

        Debug.Log(notSatified);
    }
}
