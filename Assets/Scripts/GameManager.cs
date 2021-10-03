using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class ResourceScaling
    {
        //For editor
        [SerializeField]
        String name;

        [SerializeField]
        ResourceType m_ResourceType;
        [SerializeField]
        AnimationCurve m_Scaling;
        [SerializeField]
        float m_MaxCurveTime;
        [SerializeField]
        float m_MaxNeed;
        [SerializeField]
        float m_MaxExedeRate;
        [SerializeField]
        float m_MinExedeRate;

        [SerializeField, HideInInspector]
        private float m_CurrentExedeRate;
        private float m_LastExedeRate;

        public ResourceType GetResourceType() { return m_ResourceType; }
        public float EvaluateTotalNeed(float time) { return m_Scaling.Evaluate(time / m_MaxCurveTime) * m_MaxNeed; }
        public float GetExedeRate() { return m_CurrentExedeRate; }
        public void GenerateExedeRate()
        {
            m_LastExedeRate = m_CurrentExedeRate;
            m_CurrentExedeRate += Random.Range(m_MinExedeRate, m_MaxExedeRate);
        }
        public float GetExedeDelta() { return m_CurrentExedeRate - m_LastExedeRate; }
    }

    //Editor variabels
    [SerializeField]
    Demand m_DemandEntity;

    [SerializeField]
    List<ResourceScaling> m_ResourceScaling;

    //Hiden varables
    private float m_Timer = 0f;

    EntityManager m_EntityManager;
    private void Start()
    {
        m_EntityManager = new EntityManager();

        foreach (ResourceScaling scaling in m_ResourceScaling)
        {
            scaling.GenerateExedeRate();
        }
    }

    private void Update()
    {
        m_Timer += Time.deltaTime;

        foreach (ResourceScaling scaling in m_ResourceScaling)
        {
            float total_resource_demand = scaling.EvaluateTotalNeed(m_Timer);
            float exede_rate = scaling.GetExedeRate();

            if (total_resource_demand >= exede_rate)
            {
                SpawnBuilding(scaling);
            }
        }

        m_EntityManager.Tick();
    }

    private void SpawnBuilding(ResourceScaling scaling)
    {

        Entity spawned_entity = m_EntityManager.Add(m_DemandEntity, Vector3.zero);
        if (spawned_entity is Demand spawned_demand)
        {
            Demand.Need need = new Demand.Need{ Type = scaling.GetResourceType(), Value = scaling.GetExedeDelta() };
            spawned_demand.Needs.Add(need);
            scaling.GenerateExedeRate();
        }
    }

}
