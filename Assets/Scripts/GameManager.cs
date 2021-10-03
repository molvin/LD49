using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public Grid Grid;

    private void Start()
    {
    }

    private void Update()
    {
        //Tick Grid
        //Spawn new entities
        //Do other garbage

        Grid.Tick();
    }
}
