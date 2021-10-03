using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugControlHose : MonoBehaviour
{
    public float Acceleration;
    public float Decceleration;
    private Rigidbody2D Body;
    private Vector2 Velocity;

    void Awake() => Body = GetComponent<Rigidbody2D>();
    void Update()
    {
        Velocity *= Mathf.Pow(Decceleration, Time.deltaTime);

        float Horizontal = Input.GetAxisRaw("Horizontal");
        float Vertical = Input.GetAxisRaw("Vertical");
        Vector2 dVel = new Vector2(Horizontal, Vertical).normalized * Acceleration * Time.deltaTime;
        Velocity += dVel;
    }

    void FixedUpdate()
    {
        Body.velocity = Velocity;
    }
}
