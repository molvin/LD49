using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionPoint : MonoBehaviour
{
    public enum Interactable
    {
        Socket,
        Valve,
        Hose
    }

    public Interactable InteractType;

    private Entity ParentEntity;

    private void Start()
    {
        ParentEntity = GetComponentInParent<Entity>();
    }

    public void Interact(Player player)
    {
        switch(InteractType)
        {
            case Interactable.Socket:
                if(player.CurrentState is Player.State.Hose)
                {
                    Debug.Log("Connecting hose");

                    //Player is holding a hose, try to connect it
                    Edge edge = ParentEntity.GetEdge(transform.position);
                    bool success = ParentEntity.TryConnect(player.Interaction.Edge.Value, edge);
                    if(success)
                    {
                        Debug.Log("Connected hose");

                        player.Interaction.Clear();
                        player.CurrentState = Player.State.Move;
                    }
                }
                else
                {
                    Edge edge = ParentEntity.GetEdge(transform.position);

                    if (edge.Other == null)
                    {
                        Debug.Log("Picking up hose");

                        Hose hose = player.m_EntityManager.Add(player.HosePrefab, transform.position) as Hose;

                        edge.Other = hose;
                        edge.OtherSocket = 0;
                        Edge hoseEdge = hose.Edges[0];
                        hoseEdge.Other = ParentEntity;
                        hoseEdge.OtherSocket = edge.SelfSocket;
                        hose.Edges[0] = hoseEdge;

                        hose.Socket0.position = transform.position;

                        //Create new hose
                        player.CurrentState = Player.State.Hose;
                        player.Interaction.Clear();
                        player.Interaction.Entity = hose;
                    }
                    else
                    {
                        //Pick up Hose
                    }
                }
                
                break;

            case Interactable.Valve:
                if (!(player.CurrentState is Player.State.Move))
                    break;

                //GOTO VALVE STATE
                break;
            case Interactable.Hose:
                if (!(player.CurrentState is Player.State.Move))
                    break;
                //DUNNO WHAT TO DO HERE
                break;
        }
    }
}
