using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Hose : PressurisedEnitity
{
    public Transform Socket0;
    public Transform Socket1;
    public HoseSentinel Sentinel0;
    public HoseSentinel Sentinel1;
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
            if (IncommingEdge.Self is Demand || IncommingEdge.Self is Combiner)
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

    private List<HoseJoint> Joints = new List<HoseJoint>();
    private List<HoseStick> Sticks = new List<HoseStick>();
    private List<InterpolatePoint> Points = new List<InterpolatePoint>();

    private GameObject StartSentinel;
    private GameObject EndSentinel;

    class InterpolatePoint
    {
        public Vector2 CurrentPosition;
        public Vector2 PreviousPosition;
    }

    public void Initialize(Hose Hose)
    {
        Owner = Hose;
        StickLength = Vector3.Distance(
            Owner.HoseStickPrefab.LeftSocket.transform.position,
            Owner.HoseStickPrefab.RightSocket.transform.position);
        JointRadius = Owner.HoseJointPrefab.GetComponent<CircleCollider2D>().radius * 1.2f;
        ActorRadius = Player.Instance.GetComponent<CircleCollider2D>().radius * 1.2f;
    }
    public void Update()
    {
        // EXIT
        if (StateData.StateExited)
        {
            if (CurrentJoint && ShouldCreateStick)
            {
                Joints.Remove(CurrentJoint);

                GameObject.Destroy(CurrentJoint.gameObject);
                CurrentJoint = null;
            }

            if (Sticks.Count > 0)
            {
                HingeJoint2D Hinge = Owner.Sentinel0.Hinge;
                Hinge.connectedBody = Sticks[0].Rigidbody2D;
                Hinge.enabled = true;

                Hinge = Owner.Sentinel1.Hinge;
                Hinge.connectedBody = Sticks[Sticks.Count - 1].Rigidbody2D;
                Hinge.enabled = true;
            }

            //FreezeBodies(true);

            //CreateSentinels();

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
            //DestroySentinels();
            //FreezeBodies(false);

            //HingeJoint2D Hinge = Player.Instance.Hinge;
            //Hinge.enabled = true;

            ShouldCreateStick = true;

            HoseStick Stick = HoseSocket == 0 ? SocketStick0 : SocketStick1;
            // Picking up existing.
            if (Stick)
            {
                //Hinge.connectedBody = Stick.Rigidbody2D;
                //Hinge.connectedAnchor = Stick.transform.position;

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
                Vector3 Vec = ActorLocation(HoseSocket) - ReelLocation(HoseSocket);
                Vector3 Pos = ActorLocation(HoseSocket) - Vec.normalized * (StickLength * 0.5f + ActorRadius);
                Quaternion Dir = Quaternion.FromToRotation(Vector3.right, Vec);
                HoseStick Stick = GameObject.Instantiate(Owner.HoseStickPrefab, Pos, Dir);
                SocketStick0 = Stick;
                SocketStick1 = Stick;

                var Sentinel = HoseSocket == 0 ? Owner.Sentinel0 : Owner.Sentinel1;
                Sentinel.Hinge.enabled = true;
                Sentinel.Hinge.connectedBody = Stick.Rigidbody2D;
                Sentinel.Hinge.connectedAnchor = Stick.transform.position;//GetClosestSocketLocationOnStick(CurrentStick, ActorLocation);

                Assert.IsTrue(Sticks.Count == 0);
                Assert.IsTrue(Joints.Count == 0);
                Sticks.Add(Stick);

                ShouldCreateStick = false;
            }
        }
        else
        {
            Vector3 JointLocation = CurrentJoint.transform.position;
            float Distance = Vector3.Distance(ReelLocation(HoseSocket), JointLocation);
            if (Distance >= StickLength + CurrentJoint.Radius)
            {
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
                Hinge.connectedBody = Stick.Rigidbody2D;
                Hinge.connectedAnchor = Stick.transform.position;//GetClosestSocketLocationOnStick(CurrentStick, ActorLocation);

                Assert.IsTrue(Sticks.Count != 0);
                if (HoseSocket == 0)
                {
                    Sticks.Add(Stick);
                }
                else
                {
                    Sticks.Insert(0, Stick);
                }

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
            Hinge.connectedBody = CurrentStick(HoseSocket).Rigidbody2D;
            Hinge.connectedAnchor = CurrentStick(HoseSocket).transform.position;//GetClosestSocketLocationOnStick(CurrentStick, ActorLocation);

            if (HoseSocket == 0)
            {
                Joints.Add(CurrentJoint);
            }
            else
            {
                Joints.Insert(0, CurrentJoint);
            }

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
            SocketStick0.Rigidbody2D.constraints = Constraints;
        }
        if (SocketStick1)
        {
            SocketStick1.Rigidbody2D.constraints = Constraints;
        }

        if (!ShouldFreeze)
        {
            Points.Clear();
        }
        else
        {
            // Create points.
            if (Joints.Count == 0)
            {
                // Don't even bother.
                return;
            }

            //Vector3 StartSentinel = GetFurthestSocketLocationOnStick(Sticks[0], Joints[0].transform.position);
            //Vector3 Dir = StartSentinel - Sticks[0].transform.position;
            //StartSentinel += Dir.normalized * JointRadius;
            //Points.Add(StartSentinel);

            Vector3 SocketLocation0 = Owner.Socket0.transform.position;
            Vector3 SocketLocation1 = Owner.Socket1.transform.position;
            int X = Vector3.Distance(SocketLocation0, Sticks[0].transform.position)
                < Vector3.Distance(SocketLocation1, Sticks[0].transform.position)
                ? 0 : 1;

            Points.Clear();
            // Start sentinel
            var StartPoint = new InterpolatePoint();
            StartPoint.CurrentPosition = (X == 0 ? SocketLocation0 : SocketLocation1);
            StartPoint.PreviousPosition = StartPoint.CurrentPosition;
            Points.Add(StartPoint);

            Joints.ForEach(Joint =>
            {
                var Point = new InterpolatePoint();
                Point.CurrentPosition = Joint.transform.position;
                Point.PreviousPosition = Point.CurrentPosition;
                Points.Add(Point);
            });

            // End sentinel
            var EndPoint = new InterpolatePoint();
            EndPoint.CurrentPosition = (X == 0 ? SocketLocation1 : SocketLocation0);
            EndPoint.PreviousPosition = EndPoint.CurrentPosition;
            Points.Add(EndPoint);
        }
    }

    private void CreateSentinels()
    {
        DestroySentinels();

        if (Points.Count < 2)
        {
            return;
        }


        GameObject MakeSentinel(Vector2 Position, float Size, HoseStick Stick)
        {
            Size = Mathf.Min(Size, 0.5f);
            var Sentinel = new GameObject();
            Sentinel.transform.position = Position;

            var Collider = Sentinel.AddComponent<CircleCollider2D>();
            Collider.radius = Size;

            var Body = Sentinel.AddComponent<Rigidbody2D>();
            Body.gravityScale = 0.0f;
            Body.constraints = 
                RigidbodyConstraints2D.FreezePositionX |
                RigidbodyConstraints2D.FreezePositionY |
                RigidbodyConstraints2D.FreezeRotation;

            var Hinge = Sentinel.AddComponent<HingeJoint2D>();
            Hinge.connectedBody = Stick.Rigidbody2D;
            Hinge.connectedAnchor = Stick.transform.position;//GetClosestSocketLocationOnStick(CurrentStick, ActorLocation);

            return Sentinel;
        }
        Vector3 SocketLocation0 = Owner.Socket0.transform.position;
        Vector3 SocketLocation1 = Owner.Socket1.transform.position;
        int X = Vector3.Distance(SocketLocation0, Sticks[0].transform.position)
            < Vector3.Distance(SocketLocation1, Sticks[0].transform.position)
            ? 0 : 1;

        Vector2 First = (X == 0 ? SocketLocation0 : SocketLocation1);
        Vector2 Last = (X != 0 ? SocketLocation0 : SocketLocation1);

        var FirstStick = Sticks[0];
        Vector2 Closest0 = GetClosestSocketLocationOnStick(FirstStick, First);
        StartSentinel = MakeSentinel(First, Vector2.Distance(First, Closest0) * 0.9f, FirstStick);

        var LastStick = Sticks[Sticks.Count - 1];
        Vector2 Closest1 = GetClosestSocketLocationOnStick(LastStick, Last);
        EndSentinel = MakeSentinel(Last, Vector2.Distance(Last, Closest1) * 0.9f, LastStick);
    }

    private void DestroySentinels()
    {
        if (StartSentinel)
        {
            GameObject.Destroy(StartSentinel.gameObject);
            StartSentinel = null;
        }
        if (EndSentinel)
        {
            GameObject.Destroy(EndSentinel.gameObject);
            EndSentinel = null;
        }
    }

    //private void SnapFrozenBodies()
    //{
        //if (SocketStick0 && SocketStick0.Rigidbody2D.constraints != RigidbodyConstraints2D.None)
        //{
            //SnapBody(SocketStick0, 0);
        //}
        //if (SocketStick1 && SocketStick1.Rigidbody2D.constraints != RigidbodyConstraints2D.None)
        //{
            //// Only snap if not same.
            //if (!SocketStick0 || SocketStick0 != SocketStick1)
            //{
                //SnapBody(SocketStick1, 1);
            //}
        //}
        //void SnapBody(HoseStick Stick, int Socket)
        //{
            //if (Joints.Count == 0)
            //{
                //return;
            //}

            //Vector3 SnapLocation = ActorLocation(Socket);
            //Vector3 Closest = GetClosestSocketLocationOnStick(Stick, SnapLocation);
            ////Stick.transform.position += (SnapLocation - Closest);

            //// Exclude first and last.
            //for (int i = 1; i < Points.Count - 1; i++)
            //{
                //var Point = Points[i];
                //Vector2 Previous = Point.CurrentPosition;
                //Point.CurrentPosition += Point.CurrentPosition - Point.PreviousPosition;
                //Point.PreviousPosition = Previous; 
            //}

            //float FullLength = StickLength + JointRadius * 2.0f;

            //Assert.IsTrue(Sticks.Count == Points.Count - 1);
            //for (int x = 0; x < 10; x++)
            //{
                //for (int i = 0; i < Sticks.Count; i++)
                //{
                    //Vector2 Point0 = Points[i].CurrentPosition;
                    //Vector2 Point1 = Points[i + 1].CurrentPosition;
                    //Vector2 Origin = (Point0 - Point1) * 0.5f;
                    //Vector2 Direction = (Point0 - Point1).normalized;
                    //if (i != 0)
                    //{
                        //Points[i].CurrentPosition = Origin + Direction * FullLength * 0.5f;
                    //}
                    //if (i + 1 != Sticks.Count)
                    //{
                        //Points[i + 1].CurrentPosition = Origin - Direction * FullLength * 0.5f;
                    //}
                //}
            //}

            //for (int i = 0; i < Sticks.Count; i++)
            //{
                //Vector2 Point0 = Points[i].CurrentPosition;
                //Vector2 Point1 = Points[i + 1].CurrentPosition;
                //Vector2 Origin = (Point0 - Point1) * 0.5f;
                ////Vector2 Direction = (Point0 - Point1).normalized;
                //Sticks[i].transform.position = Origin;
            //}

            //for (int i = 0; i < Joints.Count; i++)
            //{
                //Joints[i].transform.position = Points[i + 1].CurrentPosition;
            //}


            ///*
            //for (int i = 0; i < numIterations; i++)
            //{
                //foreach (Stick stick in sticks)
                //{
                    //Vector2 stickCentre = (stick.pointA.position + stick.pointB.position) / 2;
                    //Vector2 stickDir = (stick.pointA.position - sitck.pointB.position).normalized;
                    //if (!stick.pointA.locked)
                        //stick.pointA.position = stickCentre + stickDir * stick.length / 2;
                    //if (!stick.pointB.locked)
                        //stick.pointB.position = stickCentre - stickDir * stick.length / 2;
                //}
            //}
            //*/
        //}
    //}

    private Vector3 GetClosestSocketLocationOnStick(HoseStick Stick, Vector3 Location)
    {
        GameObject HingeLocation =
            Vector3.Distance(Stick.LeftSocket.transform.position, Location)
            < Vector3.Distance(Stick.RightSocket.transform.position, Location)
            ? Stick.LeftSocket
            : Stick.RightSocket;
        return (HingeLocation.transform.position);
    }
    private Vector3 GetFurthestSocketLocationOnStick(HoseStick Stick, Vector3 Location)
    {
        GameObject HingeLocation =
            Vector3.Distance(Stick.LeftSocket.transform.position, Location)
            > Vector3.Distance(Stick.RightSocket.transform.position, Location)
            ? Stick.LeftSocket
            : Stick.RightSocket;
        return (HingeLocation.transform.position);
    }

}