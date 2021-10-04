using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLogic : MonoBehaviour
{
    public Vector3 Offset;
    public float SmoothTime;
    public float NormalZoom;
    public float FarZoom;
    public float ZoomOutSmoothTime;
    public float ZoomInSmoothTime;

    private Player Player;
    private Camera Camera;


    private void Start()
    {
        Player = FindObjectOfType<Player>();
        Camera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        float zoomVelocity = 0.0f;
        if(Input.GetButton("Zoom"))
        {
            Camera.orthographicSize = Mathf.SmoothDamp(Camera.orthographicSize, FarZoom, ref zoomVelocity, ZoomOutSmoothTime);
        }
        else
        {
            Camera.orthographicSize = Mathf.SmoothDamp(Camera.orthographicSize, NormalZoom, ref zoomVelocity, ZoomInSmoothTime);

        }

        Vector3 velocity = Vector3.zero;
        transform.position = Vector3.SmoothDamp(transform.position, Player.transform.position + Offset, ref velocity, SmoothTime);
    }
}
