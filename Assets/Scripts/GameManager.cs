using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    private List<PressurisedEnitity> m_PressurisedEnititiesPrefabs;

  //  private Dictionary<T, PressurisedEnitity> m_PlasedPressurizedEntities;

    private void Start()
    {
        foreach(PressurisedEnitity pressurized_entity in m_PressurisedEnititiesPrefabs)
        {

        }
    }

    private void Update()
    {
 

    }
}
