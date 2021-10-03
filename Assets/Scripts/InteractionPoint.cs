using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionPoint : MonoBehaviour
{
    public enum Interactable
    {
        Socket,
        Valve
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
                        //Create new hose
                        player.CurrentState = Player.State.Hose;
                        player.Interaction.Clear();
                        player.Interaction.Entity = ParentEntity;
                        player.Interaction.Edge = edge;
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
        }
    }
}
