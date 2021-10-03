using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoseJoint : MonoBehaviour
{
    public HingeJoint2D LeftJoint, RightJoint;
    public float Radius => GetComponent<CircleCollider2D>().radius * 1.2f;
}
