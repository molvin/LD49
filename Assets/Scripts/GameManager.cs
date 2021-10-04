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
    public float m_MinSpawnDistanceOffset;
    public float m_MaxSpawnDistanceOffset;
    public float m_MaxSpawnDistancePerSpawn; 

    public float m_AvoidanceDistance;


    [SerializeField]
    FactoryManager m_FactoryEntity;

    [Header("Kablam")]
    public ParticleSystem m_KablamParticleSystem;

    //Hiden varables
    private float m_Timer = 0f;
    private List<int> m_OrderdUnlocks = new List<int>();
    private bool doKablaam;

    public EntityManager m_EntityManager;

    protected void Awake()
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

        if (!doKablaam)
        {
//            m_KablamParticleSystem = Instantiate(m_KablamParticleSystem, FindObjectOfType<Player>().gameObject.transform);
 //           m_KablamParticleSystem.Play();
            doKablaam = true;
        }

        WarningSystem.Instance.ClearAll();

        Debug.Log("Game Over!!");
        Time.timeScale = 0;
        //TODO: Play animation for end of level, return to main menu
        StartCoroutine(GameOverRoutine());
        IEnumerator GameOverRoutine()
        {
            float time = 20.0f + Time.realtimeSinceStartup;
            float CamStart = Camera.main.orthographicSize;
            var CamLogic = Camera.main.GetComponent<CameraLogic>();
            float CamEnd = CamLogic.FarZoom;

            CamLogic.FadeImage.gameObject.SetActive(true);

            while (Time.realtimeSinceStartup < time)
            {
                float Factor = (time - Time.realtimeSinceStartup) / 20.0f; // 1 to 0
                Camera.main.orthographicSize = Mathf.Lerp(CamEnd, CamStart, Factor);

                if (Factor < 0.75 && !CamLogic.FadeText.activeSelf)
                {
                    CamLogic.FadeText.SetActive(true);
                }
                if (Input.GetButtonDown("Interact")) goto end;

                yield return null;
            }

            for(;;) if (Input.GetButtonDown("Interact")) break; else yield return null;

            end:
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

            float random_distance = Random.Range(m_MinSpawnDistanceOffset, m_MaxSpawnDistanceOffset + (m_MaxSpawnDistancePerSpawn * m_EntityManager.Entities.Count));

            Vector3 temp_pos = random_direction * random_distance;

            if (IsPointAccesable(temp_pos, m_AvoidanceDistance))
            {
                spawn_pos = temp_pos;
                break;
            }
            if (i== 999) return;
        }

        if (spawn_pos == Vector3.negativeInfinity)
        {
            Debug.LogError("There where no valid point to spawn entity at");
            return;
        }

        Instantiate(m_FactoryEntity, spawn_pos, Quaternion.identity);
    }

    private bool IsPointAccesable(Vector3 origin, float distance)
    {
        if (Physics2D.OverlapBox(origin, new Vector2(7, 7), 0))
            return false;

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

    public void GetNeeds(ref List<Demand.Need> needs, int resource_variants_count)
    {
            int max_color_index = 1;

            int demands_count = 0;

            foreach (Entity entity in m_EntityManager.Entities)
            {
                if (entity is Demand)
                    demands_count++;
            }

            foreach (int unlock_count in m_OrderdUnlocks)
            {
                if (unlock_count > demands_count)
                    break;

              //  Debug.Log("Unlock count: " + unlock_count + ", current demand count: " + demands_count);

                max_color_index++;
            }

            ResourceType[] items = (ResourceType[])Enum.GetValues(typeof(ResourceType));

            List<ResourceType> resource_pool = new List<ResourceType>();

            for (int i = 1; i < max_color_index; i++)
            {
                ResourceType resource = items[i];
                if (needs.TrueForAll((need) => need.Type != resource))
                    resource_pool.Add(resource);
            }

            for (int i = needs.Count; i < resource_variants_count; i++)
            {
                if (resource_pool.Count == 0)
                    break;

                int resource_index = Random.Range(0, resource_pool.Count);
                Demand.Need need = new Demand.Need { Type = resource_pool[resource_index], Value = Random.Range(m_MinDemand, m_MaxDemand) };
                needs.Add(need);
                resource_pool.RemoveAt(resource_index);
            }
        
    }

    public int AvailableResources()
    {
        int demands_count = 0;
        int max_color_index = 0;
        foreach (Entity entity in m_EntityManager.Entities)
        {
            if (entity is Demand)
                demands_count++;
        }
        foreach (int unlock_count in m_OrderdUnlocks)
            {
                if (unlock_count > demands_count)
                    break;

              //  Debug.Log("Unlock count: " + unlock_count + ", current demand count: " + demands_count);

                max_color_index++;
            }
        Debug.Log("max " + max_color_index);

        return max_color_index;
    }

}
