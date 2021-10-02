using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    //States
    public enum State
    {
        Move = 0, 
        Build = 1,
        Destroy = 2,
        Interact = 3
    }
    public State CurrentState;
    //Movement
    public float AccelerationTime;
    public float DecelerationTime;
    public float MaxSpeed;
    public float MinInput;
    public Vector3 Veloctiy;
    public Transform SpriteHolder;
    public Animator Animator;
    //Building
    public enum Item
    {
        Valve = 0,
        Splitter = 1,
        Combiner = 2,
    }
    public int SelectedItem;
    public float CycleTickTime;
    private List<Item> Items;
    private float cycleTimer;

    private void Awake()
    {
        Items = ((Item[])System.Enum.GetValues(typeof(Item))).ToList();
    }

    private void Update()
    {
        switch(CurrentState)
        {
            case State.Move:
                UpdateMoveState();
                break;
            case State.Build:
                UpdateBuildState();
                Move();
                break;
            case State.Destroy:
                UpdateDestroyState();
                Move();
                break;
            case State.Interact:
                UpdateInteractState();
                break;
        }

        cycleTimer += Time.unscaledDeltaTime;
    }
    
    public List<Item> getItems()
    {
        return Items;
    }

    private void UpdateMoveState()
    {
        Move();
 
        if (Input.GetButtonDown("Build"))
            CurrentState = State.Build;
        if (Input.GetButtonDown("Destroy"))
            CurrentState = State.Destroy;
        if (Input.GetButtonDown("Interact"))
            TryInteract();
    }

    private void UpdateBuildState()
    {
        float cycle = Input.GetAxisRaw("Cycle");
        if (Mathf.Abs(cycle) > MinInput && cycleTimer > CycleTickTime)
        {
            cycleTimer = 0;
            SelectedItem += (int)Mathf.Sign(cycle);
            if (SelectedItem >= Items.Count) SelectedItem = 0;
            if (SelectedItem < 0) SelectedItem = Items.Count - 1;
            Debug.Log($"Selected item {SelectedItem}");
        }
        for (int i = 0; i < Items.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectedItem = i;
                Debug.Log($"Selected item {SelectedItem}");
            }
        }

        //TODO: show ghost for thing you are trying to place
        if(Input.GetButtonDown("Interact"))
        {
            Debug.Log("Placing Item");
        }

        if (Input.GetButtonDown("Build"))
            CurrentState = State.Move;

    }
    private void UpdateDestroyState()
    {
        //TODO: highlight the relevant cell
        if (Input.GetButtonDown("Interact"))
        {
            Debug.Log("Destroying Item");
        }
    }
    private void UpdateInteractState()
    {
        //Do something
    }
    
    private void TryInteract()
    {

    }

    private void Move()
    {
        //Movement
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));
        Vector3 acceleration = Vector3.zero;

        if (input.sqrMagnitude > MinInput * MinInput)
        {
            Veloctiy = Vector3.SmoothDamp(Veloctiy, input.normalized * MaxSpeed, ref acceleration, AccelerationTime);
            //Visual Rotation
            SpriteHolder.localRotation = Quaternion.FromToRotation(Vector3.forward, input.normalized);
        }
        else
        {
            Veloctiy = Vector3.SmoothDamp(Veloctiy, Vector3.zero, ref acceleration, DecelerationTime);
        }

        transform.position += Veloctiy * Time.deltaTime;

        //Animation
        Animator.SetFloat("Speed", Veloctiy.magnitude / MaxSpeed);
        Animator.speed = Veloctiy.magnitude / MaxSpeed;

    }

}