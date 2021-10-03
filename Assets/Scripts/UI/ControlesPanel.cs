using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlesPanel : MonoBehaviour
{
    public AnimationCurve animCurve;
    public bool isHidden = true;
    public float animTime = 1.5f;
    private float timer;
    private Vector3 startPos;

    void Start()
    {
        startPos = this.transform.position;
        timer = animTime;
    }
    private void Update()
    {
        Animate();
    }

    private void Animate()
    {
        timer += isHidden ? Time.deltaTime : -Time.deltaTime;
        timer = Mathf.Clamp(timer, 0, animTime);
        float posModi = animCurve.Evaluate(timer / animTime);
        this.transform.position = new Vector3(startPos.x + (1500 * posModi), startPos.y, startPos.z);
    }

    public void SetHidden(bool hidden)
    {
        this.isHidden = hidden;
    }
}