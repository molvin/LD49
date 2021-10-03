using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionPoint : MonoBehaviour
{
    public int Socket;

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
                    bool success = ParentEntity.TryConnect(edge, player.Interaction.Edge.Value);
                    if(success)
                    {
                        Debug.Log("Connected hose");
                        if(player.Interaction.Edge.Value.SelfSocket == 0)
                            (player.Interaction.Entity as Hose).Socket0.position = transform.position;
                        if (player.Interaction.Edge.Value.SelfSocket == 1)
                            (player.Interaction.Entity as Hose).Socket1.position = transform.position;
                        player.Interaction.Clear();
                        player.CurrentState = Player.State.Move;
                    }
                }
                else if(player.CurrentState is Player.State.Move)
                {
                    int edgeIndex = ParentEntity.GetEdgeIndex(transform.position);
                    Edge edge = ParentEntity.GetEdge(transform.position);

                    if (edge.Other == null)
                    {
                        Debug.Log("Picking up hose");

                        Hose hose = GameManager.Instance.m_EntityManager.Add(player.HosePrefab, transform.position) as Hose;
                        foreach (var point in hose.InteractionPoints)
                            point.SetActive(false);

                        edge.Other = hose;
                        edge.OtherSocket = 0;
                        Edge hoseEdge = hose.Edges[0];
                        hoseEdge.Other = ParentEntity;
                        hoseEdge.OtherSocket = edge.SelfSocket;
                        hose.Edges[0] = hoseEdge;

                        ParentEntity.SetEdge(edgeIndex, edge);
                        hose.Socket0.position = transform.position;

                        //Create new hose
                        player.CurrentState = Player.State.Hose;
                        player.Interaction.Clear();
                        player.Interaction.Entity = hose;
                        player.Interaction.Edge = hose.Edges[1];
                    }
                    else
                    {
                        //Pick up Hose
                        Debug.Log("Disconnecting hose");

                        Hose hose = edge.Other as Hose;
                        hose.Edges[edge.OtherSocket] = hose.Edges[edge.OtherSocket].Cleared();
                        ParentEntity.Edges[edge.SelfSocket] = edge.Cleared();
                        player.Interaction.Entity = hose;
                        player.Interaction.Edge = hose.Edges[edge.OtherSocket];
                        player.CurrentState = Player.State.Hose;
                        foreach (var point in ParentEntity.InteractionPoints)
                            point.SetActive(false);

                    }
                }
                
                break;

            case Interactable.Valve:
                if (!(player.CurrentState is Player.State.Move))
                    break;

                player.CurrentState = Player.State.Valve;
                player.Interaction.Entity = ParentEntity;
                player.Interaction.Handle = transform.parent;
                var handles = ParentEntity.GetComponentsInChildren<DragThing>();
                foreach (var handle in handles)
                    if (handle.transform != transform.parent)
                        player.Interaction.OtherHandle = handle.transform;

                player.Interaction.StartPosition = player.transform.position;
                player.Interaction.StartDistance = (transform.parent.position - player.transform.position).magnitude;
                player.Interaction.PreviousPosition = player.transform.position;
                break;
            case Interactable.Hose:
                if (!(player.CurrentState is Player.State.Move))
                    break;

                player.Interaction.Entity = ParentEntity;
                player.Interaction.Edge = ParentEntity.GetEdge(transform.position);
                player.CurrentState = Player.State.Hose;
                foreach (var point in ParentEntity.InteractionPoints)
                    point.SetActive(false);

                break;
        }
    }
}
