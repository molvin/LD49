using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public Grid Grid;



    private void Start()
    {
        List<int> list = new List<int>();
        var l = list.Select(x => 1);
    }

    private void Update()
    {
        //Tick Grid
        //Spawn new entities
        //Do other garbage

        Grid.Tick();
    }
}
