using UnityEngine.UI;
using UnityEngine;

public class WarningObject : MonoBehaviour
{
    string id;
    float timeInSeconds;
    float currentTime;
    private Image radialImage;

    private Transform target;

    public void constructor(Transform target, float timeInSeconds)
    {
        this.target = target;
        this.timeInSeconds = timeInSeconds;
        this.currentTime = 0;
        this.radialImage = this.transform.GetComponentInChildren<Image>();
    }

    void Update()
    {
  
        moveIndicator();
        TickTime();
    }

    private void SetRenderesEnabled(bool enabled)
    {
        Renderer[] renders =  this.GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in renders)
        {
            renderer.enabled = enabled;
        }
        Image[] images = this.GetComponentsInChildren<Image>();
        foreach (Image renderer in images)
        {
            renderer.enabled = enabled;
        }
    }
  
    private void TickTime()
    {
        currentTime += Time.deltaTime;
        float factor = currentTime / timeInSeconds;
        radialImage.fillAmount =1- factor;
        if(factor >= 1)
        {
            Debug.Log("DIE, YOU DIE!");
        }

    }

    private void moveIndicator()
    {
        Vector3 screenpos = Camera.main.WorldToScreenPoint(target.position);
        if (isTargetOnScreen(screenpos))
        {
            this.SetRenderesEnabled(false);
            //this.transform.position = Camera.main.ScreenToWorldPoint(screenpos);
        }
        else
        {
            this.SetRenderesEnabled(true);
            Vector3 center = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 10));
            float offset = 110f;
            float y = Mathf.Clamp(screenpos.y, center.y - (Screen.height / 2) + offset, center.y + (Screen.height / 2) - offset);
            float x = Mathf.Clamp(screenpos.x, center.x - (Screen.width / 2) + offset, center.x + (Screen.width / 2) - offset);
            Vector3 clampedPos = new Vector3(x, y, 0);
            this.transform.position = clampedPos;
            //this.transform.position = Camera.main.ScreenToWorldPoint(clampedPos,Camera.MonoOrStereoscopicEye.Mono);
            //this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 0);
            Vector3 dir = center - clampedPos;
            float rot = Mathf.Atan2(dir.y, dir.x);
            this.transform.rotation = Quaternion.Euler(0, 0, rot * Mathf.Rad2Deg);

            return;


            //Vector2 vectorToCenter = new Vector2(screenpos.x - center.x, screenpos.y - center.y);
            //float rot = Mathf.Atan2(vectorToCenter.y, vectorToCenter.x);
            //rot = rot * 180 / Mathf.PI;
            //this.transform.rotation = Quaternion.Euler(90, 0,  rot *Mathf.Rad2Deg);
            //float slope =  vectorToCenter.y / vectorToCenter.x;
            //float padding = 20;
            //Vector2 paddedArea = new Vector2(Screen.width-padding, Screen.height-padding);
            //Vector2 endPos;
            //if(vectorToCenter.y<0)
            //{
            //    endPos = new Vector2((-paddedArea.y / 2) / slope, (-paddedArea.y / 2));
            //} else
            //{
            //    endPos = new Vector2((paddedArea.y / 2) / slope, (-paddedArea.y / 2));
            //}
            //if (endPos.x < -paddedArea.x / 2)
            //{
            //    endPos = new Vector2(-paddedArea.y / 2 , slope * -paddedArea.y / 2);

            //} else if (endPos.x > paddedArea.x / 2)
            //{
            //    endPos = new Vector2(paddedArea.x / 2, slope * paddedArea.y / 2);

            //}
            //this.transform.position = Camera.main.ScreenToWorldPoint(endPos);

        }

    }

    bool isTargetOnScreen(Vector3 target)
    {

        if (target.y > 0 &&
          target.y < Screen.height &&
          target.x < Screen.width &&
          target.x > 0)
        {
            //the target is on-screen, use an overlay, or do nothing.
            return true;
        }
        else
        {
            //the target is off-screen, find indicator position.
            return false;
        }
    }
}
