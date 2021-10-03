using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugReel : MonoBehaviour
{
    public GameObject ReelActor;
    public float ActorRadius => ReelActor.GetComponent<CircleCollider2D>().radius * 1.2f;
    [Header("Prefabs")]
    public HoseStick HoseStick;
    public HoseJoint HoseJoint;

    private float StickLength;
    private HoseStick CurrentStick;
    private HoseJoint CurrentJoint;
    private bool ShouldCreateStick = true;

    public void Awake()
    {
        StickLength = Vector3.Distance(HoseStick.LeftSocket.transform.position, HoseStick.RightSocket.transform.position);
        ReelActor.GetComponent<HingeJoint2D>().enabled = false;
    }

    public void Update()
    {
        MaybeCreateStick();
    }

    private void MaybeCreateStick()
    {
        if (!ShouldCreateStick)
        {
            MaybeCreateJoint();
            return;
        }
        if (CurrentStick == null && CurrentJoint == null)
        {
            Vector3 ActorLocation = ReelActor.transform.position;
            float Distance = Vector3.Distance(transform.position, ActorLocation);
            if (Distance >= StickLength + ActorRadius)
            {
                ReelActor.GetComponent<HingeJoint2D>().enabled = true;

                Vector3 Vec = ActorLocation - transform.position;
                Vector3 Pos = ActorLocation - Vec.normalized * (StickLength * 0.5f + ActorRadius);
                Quaternion Dir = Quaternion.FromToRotation(Vector3.right, Vec);
                CurrentStick = Instantiate(HoseStick, Pos, Dir);

                var Hinge = ReelActor.GetComponent<HingeJoint2D>();
                Hinge.connectedBody = CurrentStick.GetComponent<Rigidbody2D>();
                Hinge.connectedAnchor = CurrentStick.transform.position;//GetClosestSocketLocationOnStick(CurrentStick, ActorLocation);

                ShouldCreateStick = false;
            }
        }
        else
        {
            Vector3 JointLocation = CurrentJoint.transform.position;
            float Distance = Vector3.Distance(transform.position, JointLocation);
            if (Distance >= StickLength + CurrentJoint.Radius)
            {
                ReelActor.GetComponent<HingeJoint2D>().enabled = true;

                Vector3 Vec = JointLocation - transform.position;
                Vector3 Pos = JointLocation - Vec.normalized * (StickLength * 0.5f + CurrentJoint.Radius);
                Quaternion Dir = Quaternion.FromToRotation(Vector3.right, Vec);
                CurrentStick = Instantiate(HoseStick, Pos, Dir);

                var Hinge = CurrentJoint.LeftJoint;
                Hinge.enabled = true;
                Hinge.connectedBody = CurrentStick.GetComponent<Rigidbody2D>();
                Hinge.connectedAnchor = CurrentStick.transform.position;//GetClosestSocketLocationOnStick(CurrentStick, ActorLocation);

                ShouldCreateStick = false;
            }

        }
    }

    private void MaybeCreateJoint()
    {
        Vector3 EndLocation = GetClosestSocketLocationOnStick(CurrentStick, transform.position);
        float Distance = Vector3.Distance(transform.position, EndLocation);
        if (Distance >= HoseJoint.Radius)
        {
            Vector3 Vec = EndLocation - transform.position;
            Vector3 Pos = EndLocation - Vec.normalized * HoseJoint.Radius;
            CurrentJoint = Instantiate(HoseJoint, Pos, Quaternion.identity);

            var Hinge = CurrentJoint.RightJoint;
            Hinge.enabled = true;
            Hinge.connectedBody = CurrentStick.GetComponent<Rigidbody2D>();
            Hinge.connectedAnchor = CurrentStick.transform.position;//GetClosestSocketLocationOnStick(CurrentStick, ActorLocation);

            ShouldCreateStick = true;
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

    private Vector3 GetClosestSocketLocationOnJoint(HoseJoint Joint, Vector3 Location)
    {
        return
            Vector2.Distance(Joint.LeftJoint.anchor, (Vector2)Location)
            < Vector2.Distance(Joint.RightJoint.anchor, (Vector2)Location)
            ? Joint.LeftJoint.anchor
            : Joint.RightJoint.anchor;
    }
}
