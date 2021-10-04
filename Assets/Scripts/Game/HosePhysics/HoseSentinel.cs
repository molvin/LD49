using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoseSentinel : MonoBehaviour
{
    public float InterpolationSpeed = 1.0f;
    public HingeJoint2D Hinge;
    public Transform Socket;
    public Vector3 DesiredPosition => Socket.transform.position;

    private Rigidbody2D Body;

    void Awake() => Body = GetComponent<Rigidbody2D>();

    void FixedUpdate()
    {
        Vector2 Direction = DesiredPosition - transform.position;
        Body.velocity = Direction * InterpolationSpeed;
    }
}
