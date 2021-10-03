using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    //Editor variabels
    [Header("Unlocks | Factory Type")]
    public int m_LevelTwoSpawnCount;
    public int m_LevelThreeSpawnCount;

    [Header("Unlocks | Factory Type")]
    public int m_CyanSpawnCountUnlock     = 0;
    public int m_MagentaSpawnCountUnlock  = 1;
    public int m_YellowSpawnCountUnlock   = 2;
    public int m_RedSpawnCountUnlock      = 3;
    public int m_GreenSpawnCountUnlock    = 4;
    public int m_BlueSpawnCountUnlock     = 5;
    public int m_BlackSpawnCountUnlock    = 6;   

    [Header("Interval")]
    public float m_MinSpawnInteval;
    public float m_MaxSpawnInteval;

    [Header("Demand")]
    public float m_MinDemand;
    public float m_MaxDemand;

    [Header("Distance")]
    public float m_MinSpawnDistance;
    public float m_MaxSpawnDistance;

    public float m_ClosestSpawnDistance;


    [SerializeField]
    Demand m_DemandEntity;


    //Hiden varables
    private float m_Timer = 0f;
    private List<int> m_OrderdUnlocks = new List<int>();

    public EntityManager m_EntityManager;
    private void Awake()
    {
        m_OrderdUnlocks.Add(m_CyanSpawnCountUnlock);
        m_OrderdUnlocks.Add(m_MagentaSpawnCountUnlock);
        m_OrderdUnlocks.Add(m_YellowSpawnCountUnlock);
        m_OrderdUnlocks.Add(m_RedSpawnCountUnlock);
        m_OrderdUnlocks.Add(m_GreenSpawnCountUnlock);
        m_OrderdUnlocks.Add(m_BlueSpawnCountUnlock);
        m_OrderdUnlocks.Add(m_BlackSpawnCountUnlock);

        Instance = this;
        m_EntityManager = new EntityManager();
        m_Timer = Random.Range(m_MinSpawnInteval, m_MaxSpawnInteval);
    }

    private void Update()
    {
        if(m_Timer <= 0)
        {
            m_Timer = Random.Range(m_MinSpawnInteval, m_MaxSpawnInteval);
            SpawnBuilding();
        }


        m_Timer -= Time.deltaTime;
        m_EntityManager.Tick();
    }

    public static bool IsGameOver = false;
    public void GameOver()
    {
        if (IsGameOver)
            return;
        IsGameOver = true;

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

    private void SpawnBuilding()
    {

        Vector3 spawn_pos = Vector3.negativeInfinity;
        for (int i = 0; i < 1000; i++)
        {
            Quaternion random_rot = Random.rotation;
            Vector3 random_direction = random_rot * Vector3.forward;
            random_direction.z = 0;
            random_direction.Normalize();

            float random_distance = Random.Range(m_MinSpawnDistance, m_MaxSpawnDistance);

            Vector3 temp_pos = random_direction * random_distance;

            if (IsPointAccesable(temp_pos, m_ClosestSpawnDistance))
            {
                spawn_pos = temp_pos;
                break;
            }
        }

        if (spawn_pos == Vector3.negativeInfinity)
        {
            Debug.LogError("There where no valid point to spawn entity at");
            return;
        }

        

        Entity spawned_entity = m_EntityManager.Add(m_DemandEntity, spawn_pos);
        if (spawned_entity is Demand spawned_demand)
        {
            int max_color_index = 1;

            int demnands_count = 0;

            foreach (Entity entity in m_EntityManager.Entities)
            {
                if (entity is Demand)
                    demnands_count++;
            }

            foreach (int unlock_count in m_OrderdUnlocks)
            {
                if (unlock_count < demnands_count)
                    break;

                max_color_index++;
            }

            int type_count = 1;

            if (demnands_count >= m_LevelTwoSpawnCount)
                type_count++;
            if (demnands_count >= m_LevelThreeSpawnCount)
                type_count++;

            Debug.Log("Type count: " + type_count + "Demand count: " + demnands_count);

            for (int i = 0; i < type_count; i++)
            {
                ResourceType resource_type = (ResourceType)Random.Range(1, 8);
                Demand.Need need = new Demand.Need { Type = resource_type, Value = Random.Range(m_MinDemand, m_MaxDemand) };
                spawned_demand.Needs.Add(need);
            }
        }
    }

    private bool IsPointAccesable(Vector3 origin, float distance)
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (Entity entity in m_EntityManager.Entities)
        {
           if( Vector3.Distance(origin, entity.transform.position) <= distance)
            {
                positions.Add(entity.transform.position);
            }
        }

        if (positions.Count == 0)
            return true;

        return false;

    }

}
