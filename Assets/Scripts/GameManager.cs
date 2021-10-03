using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Grid Grid;

    private void Start()
    {
        Instance = this;
    }

    private void Update()
    {
        //Tick Grid
        //Spawn new entities
        //Do other garbage

        Grid.Tick();
    }
}
