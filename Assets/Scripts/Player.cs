using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Player : MonoBehaviour
{
    public struct StateData
    {
        public State State;
        public bool WasInState;
        public bool StateEntered;
        public bool StateExited;
    }
    //States
    public enum State
    {
        Move = 0, 
        Build = 1,
        Destroy = 2,
        Hose = 3,
        Valve = 4
    }
    public static Player Instance;
    public State CurrentState;
    //Movement
    public float AccelerationTime;
    public float DecelerationTime;
    public float MaxSpeed;
    public float MinInput;
    public Vector3 Velocity;
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
    public LayerMask BlockerLayer;
    public float CycleTickTime;
    private List<Item> Items;
    private float cycleTimer;
    public GameObject[] Ghosts;
    public Hose HosePrefab;
    public ParticleSystem FootStepDustRight, FootStepDustLeft, Place,  Drag, PickUp;
    private bool footstepRight;
    public StateData HoseStateData = new StateData
    {
        State = State.Hose,
        WasInState = false,
        StateEntered = false,
        StateExited = false
    };
    

    //Destruction
    public GameObject DestructionCube;
    public float DestructionOffset;
    public float DestructionRadius;
    public LayerMask EntityLayer;
    
    //Interact
    public struct InteractionData
    {
        public Entity Entity;
        public Edge? Edge;
        public Transform Handle;
        public Transform OtherHandle;
        public Vector3 StartPosition;
        public float StartDistance;
        public Vector3 PreviousPosition;

        public void Clear()
        {
            Entity = null;
            Edge = null;
        }
    }
    public InteractionData Interaction;
    public float DragSpeed = 0.5f;
    public GameObject InteractionIndicator;

    private void Start()
    {

        Rb = GetComponent<Rigidbody2D>();
        //if(GameManager.Instance != null)
        //{
         
        //}
        GameManager.Instance.m_EntityManager = new EntityManager();
        Items = ((Item[])System.Enum.GetValues(typeof(Item))).ToList();
        GetComponentInChildren<ToolBarManager>().Init();
        FootStepDustRight = ParticleSystem.Instantiate(FootStepDustRight, this.transform);
        FootStepDustLeft = ParticleSystem.Instantiate(FootStepDustLeft, this.transform);
        Place = ParticleSystem.Instantiate(Place, this.transform);
        Drag = ParticleSystem.Instantiate(Drag, this.transform);
        PickUp = ParticleSystem.Instantiate(PickUp, this.transform);
        FootStepDustRight.transform.localPosition = new Vector3(-0.8f, -0.94f, 0f);
        FootStepDustLeft.transform.localPosition = new Vector3(0.7f, -0.94f, 0f);
        footstepRight = false;
    }

    private void Update()
    {
        Instance = this;

        if (Time.timeScale == 0)
            return;

        Animator.SetBool("Dragging", CurrentState == State.Valve || CurrentState == State.Hose);
        InteractionIndicator.SetActive(false);
        DestructionCube.SetActive(false);
        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log(footstepRight);
        }
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
                break;
            case State.Hose:
                UpdateHoseState();
                break;
            case State.Valve:
                UpdateValveState();
                break;
        }

        UpdateStateMachineData(ref HoseStateData);

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

        Vector3 position = transform.position + SpriteHolder.up;
        var colliders = Physics2D.OverlapCircleAll(position, InteractionRadius, InteractionLayer);
        InteractionPoint closest = null;
        float dist = 10000000000.0f;

        foreach (var coll in colliders)
        {
            float d = (position - coll.transform.position).magnitude;
            if (d < dist)
            {
                dist = d;
                closest = coll.GetComponent<InteractionPoint>();
            }
        }

        if (closest)
        {
            InteractionIndicator.SetActive(true);

            InteractionIndicator.transform.position = closest.transform.position;
        }
    }

    private void UpdateBuildState()
    {
        if (Input.GetButtonDown("Destroy"))
        {
            CurrentState = State.Destroy;

            return;
        }
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

        var coll = Placeables[SelectedItem].GetComponent<Collider2D>();
        Vector2 size = 
            (coll is BoxCollider2D Bc) 
                ? Bc.size 
                : (coll is CircleCollider2D Ci) 
                    ? new Vector2(Ci.radius, Ci.radius) 
                    : (coll is CapsuleCollider2D Cc)
                        ? Cc.size
                        : throw new System.Exception();
        Vector3 targetPosition = transform.position + SpriteHolder.up * PlaceDistance;
        bool canPlace = !Physics2D.OverlapBox(targetPosition, size, 0.0f, EntityLayer) && !Physics2D.OverlapBox(targetPosition, size, 0.0f, BlockerLayer);

        Ghosts[SelectedItem].SetActive(true);
        Ghosts[SelectedItem].transform.position = targetPosition;
        foreach (var rend in Ghosts[SelectedItem].GetComponentsInChildren<SpriteRenderer>())
        {
            rend.color = canPlace ? Color.green : Color.red;
        }


        if(Input.GetButtonDown("Interact") && canPlace)
        {
            GameManager.Instance.m_EntityManager.Add(Placeables[SelectedItem], targetPosition);
            if (!Place.isPlaying)
            {
                Place.Play();
            }
        }

        if (Input.GetButtonDown("Build"))
            CurrentState = State.Move;

    }
    private void UpdateDestroyState()
    {
        Move();

        if (Input.GetButtonDown("Destroy"))
        {
            CurrentState = State.Move;
            if (!PickUp.isPlaying)
            {
                PickUp.Play();
            }
            return;
        }
        if (Input.GetButtonDown("Build"))
        {
            CurrentState = State.Build;
            return;
        }

        //Overlap check infront of you
        Vector2 position = transform.position + SpriteHolder.up * DestructionOffset;
        var colliders = Physics2D.OverlapCircleAll(position, DestructionRadius, EntityLayer);
        Entity closest = null;
        float dist = 10000000000.0f;
        Debug.Log($"Found entities {colliders.Length}");
        Vector2 Pos = Vector2.zero;
        foreach (var coll in colliders)
        {
            Vector2 CollPos = (Vector2)coll.transform.position + coll.offset;
            float d = (position - CollPos).magnitude;
            if (d < dist)
            {
                dist = d;
                closest = coll.GetComponent<Entity>();
                Pos = CollPos;
            }
        }
        if (closest == null || closest is Resource || closest is Demand)
            return;

        DestructionCube.SetActive(true);
        DestructionCube.transform.position = Pos;

        if (Input.GetButtonDown("Interact"))
        {
            Debug.Log("Destroying Item");
            GameManager.Instance.m_EntityManager.Destroy(closest);
            //TODO: regain one item
        }

    }

    private void Interact()
    {
        //Overlap check infront of you
        Vector3 position = transform.position + SpriteHolder.up;
        var colliders = Physics2D.OverlapCircleAll(position, InteractionRadius, InteractionLayer);
        InteractionPoint closest = null;
        float dist = 10000000000.0f;

        foreach (var coll in colliders)
        {
            float d = (position - coll.transform.position).magnitude;
            if(d < dist)
            {
                var interact = coll.GetComponent<InteractionPoint>();
                if (interact.IsHoseOnGround(this))
                {
                    closest = interact;
                    dist = d;
                }
            }
        }
        if(closest != null)
        {
            closest.Interact(this);
            return;
        }

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
            Velocity = Vector3.SmoothDamp(Velocity, input.normalized * MaxSpeed, ref acceleration, AccelerationTime);
            //Animator.GetComponent<SpriteRenderer>().flipX = CurrentState == State.Hose ? input.x > 0 : input.x < 0;
            Animator.transform.localScale = input.x > 0 ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
            
            //Visual Rotation, now just save last input rotation for interaction checks
            SpriteHolder.localRotation = Quaternion.FromToRotation(Vector3.up, input.normalized);
            if (footstepRight && !FootStepDustRight.isPlaying && !FootStepDustLeft.isPlaying)
            {
                FootStepDustRight.Play();
                footstepRight = false;
            }
            else
            {
                if (!FootStepDustLeft.isPlaying && !FootStepDustRight.isPlaying)
                {
                    FootStepDustLeft.Play();
                    footstepRight = true;
                }

            }
            
        }
        else
        {
            Velocity = Vector3.SmoothDamp(Velocity, Vector3.zero, ref acceleration, DecelerationTime);
        }

        //Rb.MovePosition(transform.position + Veloctiy * Time.deltaTime);

        //Animation
        Animator.SetFloat("Speed", Velocity.magnitude / MaxSpeed);
        Animator.speed = Velocity.magnitude / MaxSpeed;
    }
    private void FixedUpdate()
    {
        Rb.velocity = Velocity;
    }


    private void UpdateHoseState()
    {
        Vector3 position = transform.position + SpriteHolder.up;
        var colliders = Physics2D.OverlapCircleAll(position, InteractionRadius, InteractionLayer);
        InteractionPoint closest = null;
        float dist = 10000000000.0f;
        Drag.Play();

        foreach (var coll in colliders)
        {
            float d = (position - coll.transform.position).magnitude;
            if (d < dist)
            {
                dist = d;
                closest = coll.GetComponent<InteractionPoint>();
            }
        }

        if (closest && closest.InteractType == InteractionPoint.Interactable.Socket)
        {
            InteractionIndicator.SetActive(true);

            InteractionIndicator.transform.position = closest.transform.position;
        }



        Move();
        Hose Hose = Interaction.Entity as Hose;
        if (Input.GetButton("Interact"))
        {
            if (Interaction.Edge.Value.SelfSocket == 0)
                Hose.Socket0.position = transform.position;
            else
                Hose.Socket1.position = transform.position;

        }
        else
        {
            Interact();
            if(CurrentState == State.Hose)
            {
                foreach (var point in Interaction.Entity.InteractionPoints)
                {
                    //Should be active if their edge is not connected to anything
                    var ip = point.GetComponent<InteractionPoint>();
                    bool shouldBeActive = Hose.Edges[ip.Socket].Other == null;
                    point.SetActive(shouldBeActive);
                }
                Interaction.Clear();
                Drag.Stop();
                CurrentState = State.Move;
            }
        }

    }

    private void UpdateValveState()
    {
        if(Input.GetButton("Interact"))
        {
            GetComponent<CircleCollider2D>().enabled = false;
            Velocity = Vector3.zero;
            Valve valve = Interaction.Entity as Valve;

            Vector3 direction = (Interaction.Handle.GetChild(0).position - Interaction.Entity.transform.position).normalized;
            float distance = ((Interaction.Handle.localPosition).magnitude + Vector3.Dot(transform.position - Interaction.PreviousPosition, direction)) / valve.MaxDistance;
            Interaction.PreviousPosition = transform.position;
            valve.Gate = Mathf.Lerp(0, 100, distance);

            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            float dot = Vector3.Dot(direction, input);

            Animator.speed = 0.0f;
            if (input.magnitude > MinInput && dot > 0.7f && distance < 1.0f)
            {
                Velocity = direction * DragSpeed;
                Animator.speed = 1.0f;
            }
            if (input.magnitude > MinInput && dot < -0.7f && distance > 0.0f)
            {
                Velocity = -direction * DragSpeed;
                Animator.speed = 1.0f;
            }
        }
        else
        {
            GetComponent<CircleCollider2D>().enabled = true;
            CurrentState = State.Move;
            Interaction.Clear();
        }
    }

    private void UpdateStateMachineData(ref StateData Data)
    {
        if (CurrentState == Data.State)
        {
            Data.StateEntered = !Data.WasInState;
            Data.WasInState = true;
            Data.StateExited = false;
        }
        else if (Data.WasInState)
        {
            Data.WasInState = false;
            Data.StateEntered = false;
            Data.StateExited = true;
        }
        else
        {
            Data.StateEntered = false;
            Data.StateExited = false;
        }
    }
}
