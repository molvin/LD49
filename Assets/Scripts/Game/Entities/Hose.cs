using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.U2D;

public class Hose : PressurisedEnitity
{
    public Transform Socket0;
    public Transform Socket1;
    public HoseSentinel Sentinel0;
    public HoseSentinel Sentinel1;
    public CircleCollider2D Collider0;
    public CircleCollider2D Collider1;
    [Header("Prefabs")]
    public HoseStick HoseStickPrefab;
    public HoseJoint HoseJointPrefab;

    private SpriteShapeController SpriteController;
    private SpriteShapeRenderer Renderer;

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
        SpriteController = GetComponent<SpriteShapeController>();
        SpriteController.spline.Clear();

        Renderer = GetComponent<SpriteShapeRenderer>();
    }

    private void Update()
    {
        //Temp
        //LineRenderer lr = GetComponent<LineRenderer>();
        //lr.SetPositions(new Vector3[]{ Socket0.position, Socket1.position });

        Reel.Update();
        FixSpline();
        if (Type != ResourceType.None && Pressure > 0.0f)
        {
            Renderer.color = Color.Lerp(ResourceColor.TypeColor(Type), Color.white, 0.5f);
        }
        else
        {
            Renderer.color = Color.white;
        }

        Collider0.offset = transform.InverseTransformPoint(Socket0.transform.position);
        Collider1.offset = transform.InverseTransformPoint(Socket1.transform.position);
    }

    private void FixSpline()
    {
        /*
        0 - Tube
        1 - Left Unsocket
        2 - Right Unsocket
        3 - Left Socket
        4 - Right Socket
        */
        var Spline = SpriteController.spline;
        Spline.Clear();

        if (Reel.Joints.Count > 0)
        {
            for (int i = 0; i < Reel.Sticks.Count; i+=2)
            {
                HoseStick S(int Index) => Reel.Sticks[Index];

                int ind = i / 2;

                if (ind > 0 && ind <= Spline.GetPointCount())
                {
                    Vector2 Pos = Spline.GetPosition(ind - 1);
                    if (Vector2.Distance(Pos, S(i).transform.position) <= 1.5f)
                    {
                        continue;
                    }
                }

                Add(i / 2, S(i).transform.position);
                if (i == 0)
                {
                    //Spline.SetRightTangent(0, Socket0.position);
                    //if (Reel.Sticks.Count > 1)
                    //{
                        //Spline.SetLeftTangent(
                            //0, S(1).transform.position);
                    //}
                }
                else if (i == Reel.Sticks.Count - 1)
                {
                    //Spline.SetLeftTangent(i / 2, Socket1.position);
                    //Spline.SetRightTangent(
                        //(i / 2),
                        //S(i - 1).transform.position);
                }
                else
                {
                    Spline.SetRightTangent(
                        i / 2,
                        S(i).transform.right * Reel.StickLength);
                    Spline.SetLeftTangent(
                        i / 2,
                        -S(i).transform.right * Reel.StickLength);
                    //Vector3 LeftProjection = Vector3.Project(
                        //S(i - 1).transform.position,
                        //S(i).transform.right);
                    //Vector3 RightProjection = Vector3.Project(
                        //S(i + 1).transform.position,
                        //S(i).transform.right);
                    //Spline.SetLeftTangent(i / 2, LeftProjection);
                    //Spline.SetRightTangent(i / 2, RightProjection);
                }
            }
        }
        
        Add(0, Socket0.position);
        if (Vector3.Distance(Socket0.position, Socket1.position) > 2.5f)
        {
            Add(Spline.GetPointCount(), Socket1.position);

            Vector3 SocketPos0 = T(Socket0.position);
            Vector3 SocketPos1 = T(Socket1.position);
            Vector3 Second = (Spline.GetPosition(1));
            Vector3 AlmostLast = (Spline.GetPosition(Spline.GetPointCount() - 2));
            // Add intermeediate points
            if (Vector3.Distance(SocketPos0, Second) > 2.5f)
            {
                Vector3 Dir = (Second - SocketPos0).normalized;
                Add(1, Socket0.transform.position + Dir);
                Spline.SetLeftTangent(1, Dir);
                Spline.SetRightTangent(1, Dir);
            }
            if (Vector3.Distance(SocketPos1, AlmostLast) > 2.5f)
            {
                Vector3 Dir = (AlmostLast - SocketPos1).normalized;
                Add(Spline.GetPointCount() - 1, Socket1.transform.position + Dir);
                Spline.SetLeftTangent(Spline.GetPointCount() - 2, Dir);
                Spline.SetRightTangent(Spline.GetPointCount() - 2, Dir);
            }
        }
        
        if (Spline.GetPointCount() == 1)
        {
            Spline.SetSpriteIndex(0, 2);
        }

        if (Spline.GetPointCount() == 2)
        {
            Spline.SetSpriteIndex(0, 2);
            Spline.SetSpriteIndex(1, 3);
        }

        if (Spline.GetPointCount() > 2)
        {
            int LeftIndex = 0;
            int RightIndex = Spline.GetPointCount() - 2;
            int LeftSprite = Edges[0].Other ? 3 : 1;
            int RightSprite = Edges[1].Other ? 4 : 2;

            Spline.SetSpriteIndex(LeftIndex, LeftSprite);
            Spline.SetSpriteIndex(RightIndex, RightSprite);
        }

        void Add(int I, Vector3 P)
        {
            SpriteController.spline.InsertPointAt(I, T(P));
            SpriteController.spline.SetTangentMode(I, ShapeTangentMode.Continuous);
            SpriteController.spline.SetSpriteIndex(I, 0);
        }
        Vector3 T(Vector3 P) => P - transform.position;
    }
}

public class Reel
{
    public Hose Owner;
    private HoseStick SocketStick0;
    private HoseStick SocketStick1;
    private HoseJoint CurrentJoint;
    private bool ShouldCreateStick = true;
    [HideInInspector]
    public float StickLength;
    private float ActorRadius;
    private float JointRadius;
    private Player.StateData StateData => Player.Instance.HoseStateData;
    private Player.State CurrentState => Player.Instance.CurrentState;
    private HoseStick CurrentStick(int HoseSocket) => HoseSocket != 0 ? SocketStick0 : SocketStick1;
    private Vector3 ActorLocation(int HoseSocket) => HoseSocket == 0 ? Owner.Socket0.position : Owner.Socket1.position;
    private Vector3 ReelLocation(int HoseSocket) => HoseSocket != 0 ? Owner.Socket0.position : Owner.Socket1.position;

    [HideInInspector]
    public List<HoseJoint> Joints = new List<HoseJoint>();
    [HideInInspector]
    public List<HoseStick> Sticks = new List<HoseStick>();
    [HideInInspector]
    public List<InterpolatePoint> Points = new List<InterpolatePoint>();

    private GameObject StartSentinel;
    private GameObject EndSentinel;

    public class InterpolatePoint
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
        CreatePoints();
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
                Stick.transform.SetParent(Owner.transform);
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
                Stick.transform.SetParent(Owner.transform);
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
            CurrentJoint.transform.SetParent(Owner.transform);

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
        }
    }

    private void CreatePoints()
    {
        // Create points.
        if (Joints.Count == 0)
        {
            // Don't even bother.
            return;
        }

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