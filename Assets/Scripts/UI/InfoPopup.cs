using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPopup : MonoBehaviour
{

    public Text infoBody;
    public Text infoTitle;
    public Image infoImage;

    public AnimationCurve animCurve;
    public float animTime = 1;
    private float animTimer = 0;
    private bool hide = false;
    private Vector3 startScale;
    // Start is called before the first frame update
    public void Start()
    {
        startScale = this.transform.localScale;
    }
    public void Init(string title, string body, Sprite sprite)
    {
        infoTitle.text = title;
        infoBody.text = body;
        infoImage.sprite = sprite;
    }
    public void Update()
    {
        animTimer += !hide ? Time.deltaTime : -Time.deltaTime;
        animTimer = Mathf.Clamp(animTimer, 0, animTime);
        float factor = animCurve.Evaluate(animTimer/animTime);
        this.transform.localScale = new Vector3(startScale.x * factor, startScale.y * factor, startScale.z);
        if (Input.GetButtonDown("Interact") && hide == false)
        {
            hide = true;
            StartCoroutine(DestroyAfterAnim());
        }

    }
    private IEnumerator DestroyAfterAnim()
    {
        yield return new WaitForSeconds(animTime);
        Destroy(this.gameObject);

    }
}
