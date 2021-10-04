using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public ParticleSystem Kablaam;
    private bool doKablaam;

    [System.Serializable]
    public class ResourceScaling
    {
        //For editor
        [SerializeField]
        String name;

        [SerializeField]
        ResourceType m_ResourceType;
        [Header("Spawn Rate")]
        [SerializeField]
        AnimationCurve m_SpawnRateScaling;
        [SerializeField]
        float m_MaxCurveTime;
        [SerializeField]
        float m_MaxNeed;
        [SerializeField]
        float m_MaxExedeRate;
        [SerializeField]
        float m_MinExedeRate;

        [SerializeField, Header("Need")]
        AnimationCurve m_RanomdRateScaling;
        [SerializeField]
        float m_BaseNeed;
        [SerializeField]
        float m_MaxVariation;

        [SerializeField, HideInInspector]
        private float m_CurrentExedeRate;
        private float m_LastExedeRate;

        public ResourceType GetResourceType() { return m_ResourceType; }
        public float EvaluateTotalNeed(float time) { return m_SpawnRateScaling.Evaluate(time / m_MaxCurveTime) * m_MaxNeed; }
        public float GetExedeRate() { return m_CurrentExedeRate; }
        public void GenerateExedeRate()
        {
            m_LastExedeRate = m_CurrentExedeRate;
            m_CurrentExedeRate += Random.Range(m_MinExedeRate, m_MaxExedeRate);
        }
        public float GetExedeDelta() { return m_CurrentExedeRate - m_LastExedeRate; }
        public float GetNeed(float time)
        {
            float random_need_variation = m_SpawnRateScaling.Evaluate(time / m_MaxCurveTime) * Random.Range(-m_MaxVariation, m_MaxVariation);
            float need = m_BaseNeed + random_need_variation;

            return Mathf.Max(need, 0);
        }

    }

    //Editor variabels
    [SerializeField]
    Demand m_DemandEntity;

    [SerializeField]
    List<ResourceScaling> m_ResourceScaling;

    //Hiden varables
    private float m_Timer = 0f;

    public EntityManager m_EntityManager;
    private void Awake()
    {
        Instance = this;
        m_EntityManager = new EntityManager();

        doKablaam = false;

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

    public static bool IsGameOver = false;
    public void GameOver()
    {
        if (IsGameOver)
            return;
        IsGameOver = true;

        if (!doKablaam)
        {
            Kablaam = ParticleSystem.Instantiate(Kablaam, FindObjectOfType<Player>().gameObject.transform);
            Kablaam.Play();
            doKablaam = true;
        }

        Debug.Log("Game Over!!");
        Time.timeScale = 0;
        //TODO: Play animation for end of level, return to main menu
        StartCoroutine(GameOverRoutine());
        IEnumerator GameOverRoutine()
        {
            yield return new WaitForSecondsRealtime(5.0f);  //TODO: Do zomomeout and stuff

            SceneManager.LoadScene(0);
        }

    }

    private void SpawnBuilding(ResourceScaling scaling)
    {
        Quaternion random_rot = Random.rotation;
        Vector3 random_direction = random_rot * Vector3.forward;
        random_direction.z = 0;
        random_direction.Normalize();

        float random_distance = Random.Range(1f, 15f);

        Entity spawned_entity = m_EntityManager.Add(m_DemandEntity, random_direction * random_distance);
        if (spawned_entity is Demand spawned_demand)
        {
            Demand.Need need = new Demand.Need{ Type = scaling.GetResourceType(), Value = scaling.GetNeed(m_Timer) };
            spawned_demand.Needs.Add(need);
            scaling.GenerateExedeRate();
        }
    }

}
