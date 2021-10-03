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
        Hose = 3,
        Valve = 4
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
    public Rigidbody2D Rb;
    //Building
    public enum Item
    {
        Valve = 0,
        Splitter = 1,
        Combiner = 2,
    }
    public int SelectedItem;
    public List<PressurisedEnitity> Placeables;
    public float PlaceDistance;
    public float InteractionRadius = 3;
    public float InteractionDistance = 1;
    public LayerMask InteractionLayer;
    public float CycleTickTime;
    private List<Item> Items;
    private float cycleTimer;
    private GameObject[] Ghosts;
    public Hose HosePrefab;
    
    //Interact
    public struct InteractionData
    {
        public Entity Entity;
        public Edge? Edge;

        public void Clear()
        {
            Entity = null;
            Edge = null;
        }
    }
    public InteractionData Interaction;

    private void Start()
    {
        Rb = GetComponent<Rigidbody2D>();
        GameManager.Instance.m_EntityManager = new EntityManager();
        Items = ((Item[])System.Enum.GetValues(typeof(Item))).ToList();
        Ghosts = new GameObject[Items.Count];
        for(int i = 0; i < Items.Count; i++)
        {
            var ent = Instantiate(Placeables[i]);
            var go = ent.gameObject;
            Destroy(ent);
            foreach (var coll in go.GetComponentsInChildren<Collider2D>())
                Destroy(coll);
            Ghosts[i] = go;
            go.name += "-Ghost";
            go.SetActive(false);
        }

        GetComponentInChildren<ToolBarManager>().Init();
    }

    private void Update()
    {
        foreach (var g in Ghosts) g.SetActive(false);
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
            case State.Hose:
                UpdateHoseState();
                break;
            case State.Valve:
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
            Interact();
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

        //Render Ghost
        Vector3 targetPosition = transform.position + SpriteHolder.up * PlaceDistance;
        Ghosts[SelectedItem].SetActive(true);
        Ghosts[SelectedItem].transform.position = targetPosition;


        if(Input.GetButtonDown("Interact"))
        {
            GameManager.Instance.m_EntityManager.Add(Placeables[SelectedItem], targetPosition);
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
    private void Interact()
    {
        //Overlap check infront of you
        Vector3 position = transform.position + SpriteHolder.up;
        var colliders = Physics2D.OverlapCircleAll(position, InteractionRadius, InteractionLayer);
        InteractionPoint closest = null;
        float dist = 10000000000.0f;
        Debug.Log($"Try Interact {colliders.Length}");

        foreach (var coll in colliders)
        {
            float d = (position - coll.transform.position).magnitude;
            if(d < dist)
            {
                dist = d;
                closest = coll.GetComponent<InteractionPoint>();
            }
        }
        
        if(closest != null)
        {
            Debug.Log($"Interacting with {closest}");
            closest.Interact(this);
        }

    }

    private void Move()
    {
        //Movement
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector3 acceleration = Vector3.zero;

        if (input.sqrMagnitude > MinInput * MinInput)
        { 
            Veloctiy = Vector3.SmoothDamp(Veloctiy, input.normalized * MaxSpeed, ref acceleration, AccelerationTime);
            //Visual Rotation
            SpriteHolder.localRotation = Quaternion.FromToRotation(Vector3.up, input.normalized);
        }
        else
        {
            Veloctiy = Vector3.SmoothDamp(Veloctiy, Vector3.zero, ref acceleration, DecelerationTime);
        }

        //Rb.MovePosition(transform.position + Veloctiy * Time.deltaTime);

        //Animation
        Animator.SetFloat("Speed", Veloctiy.magnitude / MaxSpeed);
        Animator.speed = Veloctiy.magnitude / MaxSpeed;
    }
    private void FixedUpdate()
    {
        Rb.velocity = Veloctiy;
    }


    private void UpdateHoseState()
    {
        if(Interaction.Edge.Value.SelfSocket == 0)
            (Interaction.Entity as Hose).Socket0.position = transform.position;
        else
            (Interaction.Entity as Hose).Socket1.position = transform.position;

        if (Input.GetButtonDown("Interact"))
            Interact();

        if(Input.GetButtonDown("Destroy"))
        {
            foreach (var point in Interaction.Entity.InteractionPoints)
                point.SetActive(true);
            Interaction.Clear();
            CurrentState = State.Move;
        }

        Move();
    }
}
