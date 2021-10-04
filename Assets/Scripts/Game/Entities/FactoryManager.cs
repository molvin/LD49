using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FactoryManager : MonoBehaviour
{
    [Header("Gameobjects")]
    [SerializeField]
    private GameObject m_SmallFactory;
    [SerializeField]
    private GameObject m_MediumFactory;
    [SerializeField]
    private GameObject m_BigFactory;

    [Header("Upgrade Times")]
    [SerializeField]
    private float m_UpgradeTimeMediumFactory = 20;
    [SerializeField]
    private float m_UpgradeTimeBigFactory = 30;

    //Hiden variables
    private List<FactoryWithNeeds> m_FactoriesWithNeeds;

    private class FactoryWithNeeds
    {
        private GameObject m_Factory;
        private Demand m_Demand;
        private float m_SatesfactionTime;
        private float m_UpgradeThreshold;
        private int m_NeedCount;
        public FactoryWithNeeds(GameObject factory, Demand demand, float upgrade_threshold, int need_count)
        {
            m_Factory = factory;
            m_Demand = demand;
            m_SatesfactionTime = 0;
            m_UpgradeThreshold = upgrade_threshold;
            m_Factory.SetActive(false);
            m_Demand.ShouldAddOnActivation = false;
            m_NeedCount = need_count;
        }

        public bool IsSatisfied() { return m_Demand.IsSatisfied; }
        public bool IsEnabled() { return m_Factory && m_Factory.activeSelf; }
        public void TickSatesfaction(float delta_time) { m_SatesfactionTime += delta_time; }
        public bool ShouldUpgrade() { return m_SatesfactionTime >= m_UpgradeThreshold; }
        public List<Demand.Need> GetNeeds() { return m_Demand.Needs; }
        public void SetActive(bool active, List<Demand.Need> prerquisit_needs)
        {
            if (active)
            {
                m_Demand.Clear();

                m_Demand.Needs = prerquisit_needs;

                GameManager.Instance.GetNeeds(ref m_Demand.Needs, m_NeedCount);
                GameManager.Instance.m_EntityManager.Add(m_Demand, m_Factory.transform.position, false);
            }
            else
                GameManager.Instance.m_EntityManager.Destroy(m_Demand);

            m_Factory.SetActive(active);
        }
    }

    public FactoryManager(Vector3 pos)
    {
        transform.position = pos;
    }

    void Start()
    {
        m_FactoriesWithNeeds = new List<FactoryWithNeeds>();

        m_SmallFactory.SetActive(false);
        m_MediumFactory.SetActive(false);
        m_BigFactory.SetActive(false);

        m_FactoriesWithNeeds.Add(new FactoryWithNeeds(m_SmallFactory, m_SmallFactory.GetComponentInChildren<Demand>(true), m_UpgradeTimeMediumFactory, 1));
        m_FactoriesWithNeeds.Add(new FactoryWithNeeds(m_MediumFactory, m_MediumFactory.GetComponentInChildren<Demand>(true), m_UpgradeTimeBigFactory, 2));
        m_FactoriesWithNeeds.Add(new FactoryWithNeeds(m_BigFactory, m_BigFactory.GetComponentInChildren<Demand>(true), float.MaxValue, 3));

        m_FactoriesWithNeeds[0].SetActive(true, new List<Demand.Need>());
    }

    // Update is called once per frame
    void Update()
    {
        //We do not upgrade the last factory so therefor we skip it
        for (int i = 0; i < m_FactoriesWithNeeds.Count - 1; i++)
        {
            FactoryWithNeeds factory = m_FactoriesWithNeeds[i];

            if (!factory.IsEnabled() || !factory.IsSatisfied())
                continue;

            factory.TickSatesfaction(Time.deltaTime);

            if (factory.ShouldUpgrade())
            {
                factory.SetActive(false, new List<Demand.Need>());
                FactoryWithNeeds next_factory = m_FactoriesWithNeeds[i + 1];
                next_factory.SetActive(true, factory.GetNeeds());
            }
        }
    }
}
