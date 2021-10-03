using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hose : PressurisedEnitity
{
    public Transform Socket0;
    public Transform Socket1;
    [Header("Prefabs")]
    public HoseStick HoseStickPrefab;
    public HoseJoint HoseJointPrefab;

    private Reel Reel;

    public void UpdateSocketPositions(Vector3 start, Vector3 end)
    {
        Socket0.position = start;
        Socket1.position = end;
    }

    public override bool CanConnect(Edge TryEdge, Edge IncommingEdge)
    {
        if (TryEdge.Other == null && IncommingEdge.Self != TryEdge.Self)
        {
            if (IncommingEdge.Self is Demand)
            {
                return true;
            }
            if (IncommingEdge.Self is PressurisedEnitity Press)
            {
                return Type == ResourceType.None || Press.Type == ResourceType.None || Press.Type == Type;
            }
        }
        return false;
    }

    void Awake()
    {
        Edges = new List<Edge>(2);
        Edge Edge = new Edge { Self = this, SelfSocket = 0, Other = null, OtherSocket = -1 };
        for (int i = 0; i < 2; i++)
        {
            Edge.SelfSocket = i;
            Edges.Add(Edge);
        }

        Reel = new Reel();
        Reel.Initialize(this);
    }

    private void Update()
    {
        //Temp
        LineRenderer lr = GetComponent<LineRenderer>();
        lr.SetPositions(new Vector3[]{ Socket0.position, Socket1.position });

        Reel.Update();
    }
}

class Reel
{
    public Hose Owner;
    private HoseStick SocketStick0;
    private HoseStick SocketStick1;
    private HoseJoint CurrentJoint;
    private bool ShouldCreateStick = true;
    private float StickLength;
    private float ActorRadius;
    private float JointRadius;
    private Player.StateData StateData => Player.Instance.HoseStateData;
    private Player.State CurrentState => Player.Instance.CurrentState;
    private HoseStick CurrentStick(int HoseSocket) => HoseSocket != 0 ? SocketStick0 : SocketStick1;
    private Vector3 ActorLocation(int HoseSocket) => HoseSocket == 0 ? Owner.Socket0.position : Owner.Socket1.position;
    private Vector3 ReelLocation(int HoseSocket) => HoseSocket != 0 ? Owner.Socket0.position : Owner.Socket1.position;

    public void Initialize(Hose Hose)
    {
        Owner = Hose;
        StickLength = Vector3.Distance(
            Owner.HoseStickPrefab.LeftSocket.transform.position,
            Owner.HoseStickPrefab.RightSocket.transform.position);
        JointRadius = Owner.HoseJointPrefab.GetComponent<CircleCollider2D>().radius;
        ActorRadius = Player.Instance.GetComponent<CircleCollider2D>().radius;
    }
    public void Update()
    {
        // EXIT
        if (StateData.StateExited)
        {
            HingeJoint2D Hinge = Player.Instance.Hinge;
            Hinge.connectedBody = null;
            Hinge.enabled = false;

            if (CurrentJoint && ShouldCreateStick)
            {
                GameObject.Destroy(CurrentJoint.gameObject);
                CurrentJoint = null;
            }

            FreezeBodies(true);

            return;
        }
        if (CurrentState != Player.State.Hose)
        {
            return;
        }

        int HoseSocket = Player.Instance.Interaction.Edge.Value.SelfSocket;
        // ENTER
        if (StateData.StateEntered)
        {
            FreezeBodies(false);

            HingeJoint2D Hinge = Player.Instance.Hinge;
            Hinge.enabled = true;

            ShouldCreateStick = true;

            HoseStick Stick = HoseSocket == 0 ? SocketStick0 : SocketStick1;
            // Picking up existing.
            if (Stick)
            {
                Hinge.connectedBody = Stick.GetComponent<Rigidbody2D>();
                Hinge.connectedAnchor = Stick.transform.position;

                ShouldCreateStick = false;
            }
        }
        // UPDATE
        else
        {
            MaybeCreateStick(HoseSocket);
        }
    }
    private void MaybeCreateStick(int HoseSocket)
    {
        if (!ShouldCreateStick)
        {
            MaybeCreateJoint(HoseSocket);
            return;
        }
        // FIRST
        if (CurrentStick(HoseSocket) == null && CurrentJoint == null)
        {
            float Distance = Vector3.Distance(ReelLocation(HoseSocket), ActorLocation(HoseSocket));
            if (Distance >= StickLength + ActorRadius)
            {
                Player.Instance.GetComponent<HingeJoint2D>().enabled = true;

                Vector3 Vec = ActorLocation(HoseSocket) - ReelLocation(HoseSocket);
                Vector3 Pos = ActorLocation(HoseSocket) - Vec.normalized * (StickLength * 0.5f + ActorRadius);
                Quaternion Dir = Quaternion.FromToRotation(Vector3.right, Vec);
                HoseStick Stick = GameObject.Instantiate(Owner.HoseStickPrefab, Pos, Dir);
                SocketStick0 = Stick;
                SocketStick1 = Stick;

                HingeJoint2D Hinge = Player.Instance.Hinge;
                Hinge.connectedBody = Stick.GetComponent<Rigidbody2D>();
                Hinge.connectedAnchor = Stick.transform.position;//GetClosestSocketLocationOnStick(CurrentStick, ActorLocation);

                ShouldCreateStick = false;
            }
        }
        else
        {
            Vector3 JointLocation = CurrentJoint.transform.position;
            float Distance = Vector3.Distance(ReelLocation(HoseSocket), JointLocation);
            if (Distance >= StickLength + CurrentJoint.Radius)
            {
                Player.Instance.GetComponent<HingeJoint2D>().enabled = true;

                Vector3 Vec = JointLocation - ReelLocation(HoseSocket);
                Vector3 Pos = JointLocation - Vec.normalized * (StickLength * 0.5f + CurrentJoint.Radius);
                Quaternion Dir = Quaternion.FromToRotation(Vector3.right, Vec);
                var Stick = GameObject.Instantiate(Owner.HoseStickPrefab, Pos, Dir);
                if (HoseSocket == 0)
                {
                    SocketStick1 = Stick;
                }
                else
                {
                    SocketStick0 = Stick;
                }

                var Hinge = CurrentJoint.LeftJoint;
                Hinge.enabled = true;
                Hinge.connectedBody = Stick.GetComponent<Rigidbody2D>();
                Hinge.connectedAnchor = Stick.transform.position;//GetClosestSocketLocationOnStick(CurrentStick, ActorLocation);

                ShouldCreateStick = false;
            }
        }
    }

    private void MaybeCreateJoint(int HoseSocket)
    {
        Vector3 EndLocation = GetClosestSocketLocationOnStick(CurrentStick(HoseSocket), ReelLocation(HoseSocket));
        float Distance = Vector3.Distance(ReelLocation(HoseSocket), EndLocation);
        if (Distance >= JointRadius)
        {
            Vector3 Vec = EndLocation - ReelLocation(HoseSocket);
            Vector3 Pos = EndLocation - Vec.normalized * JointRadius;
            CurrentJoint = GameObject.Instantiate(Owner.HoseJointPrefab, Pos, Quaternion.identity);

            var Hinge = CurrentJoint.RightJoint;
            Hinge.enabled = true;
            Hinge.connectedBody = CurrentStick(HoseSocket).GetComponent<Rigidbody2D>();
            Hinge.connectedAnchor = CurrentStick(HoseSocket).transform.position;//GetClosestSocketLocationOnStick(CurrentStick, ActorLocation);

            ShouldCreateStick = true;
        }
    }

    private void FreezeBodies(bool ShouldFreeze)
    {
            RigidbodyConstraints2D FreezeConstraints =
                RigidbodyConstraints2D.FreezePositionX |
                RigidbodyConstraints2D.FreezePositionY |
                RigidbodyConstraints2D.FreezeRotation;
            RigidbodyConstraints2D UnfreezeConstraintes = RigidbodyConstraints2D.None;
            
            var Constraints = ShouldFreeze ? FreezeConstraints : UnfreezeConstraintes;

            if (SocketStick0)
            {
                SocketStick0.GetComponent<Rigidbody2D>().constraints = Constraints;
            }
            if (SocketStick1)
            {
                SocketStick1.GetComponent<Rigidbody2D>().constraints = Constraints;
            }

    }

    private Vector3 GetClosestSocketLocationOnStick(HoseStick Stick, Vector3 Location)
    {
        GameObject HingeLocation =
            Vector3.Distance(Stick.LeftSocket.transform.position, Location)
            < Vector3.Distance(Stick.RightSocket.transform.position, Location)
            ? Stick.LeftSocket
            : Stick.RightSocket;
        return (HingeLocation.transform.position);
    }

}