using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleFade : MonoBehaviour
{
    public static CircleFade Instance;
    private Image img;

    private bool isFade = false;
    private bool isOverFade = false;

    private float width;
    private float height;

    private float childTime = 0f;

    private float time = 0f;
    private float fadeTime = 1.7f;
    private float widthFadeSpeed = 250f;
    private float heightFadeSpeed = 118f;

    private void Awake()
    {
        Instance = this;
        img = GetComponent<Image>();
    }

    private void Start()
    {
        time = 0f;
        isFade = false;
        isOverFade = false;

        width = img.rectTransform.sizeDelta.x;
        height = img.rectTransform.sizeDelta.y;
    }

    private void Update()
    {
        if(isFade)
        {
            if(time >= fadeTime)
            {
                time = 0;
                isFade = false;
                transform.Find("Child").gameObject.SetActive(false);
                GameManager.Instance.clearPanel.SetActive(true);
                GameManager.Instance.ClearSet();
                gameObject.SetActive(false);
            }
            else
            {
                Image child = transform.Find("Child").GetComponent<Image>();

                Color alpha = child.color;
                
                time += Time.deltaTime;

                if(alpha.a < 1f)
                {
                    childTime += Time.deltaTime / 3f;
                    alpha.a = Mathf.Lerp(0, 1, childTime);
                    child.color = alpha;
                }

                if (width >= 30f)
                    width -= Time.deltaTime * widthFadeSpeed;

                if (height >= 15f)
                    height -= Time.deltaTime * heightFadeSpeed;

                img.rectTransform.sizeDelta = new Vector2(width, height);
            }
        }

        if (isOverFade)
        {
            if (time >= 0.7f)
            {
                time = 0;
                isOverFade = false;
                img.enabled = false;
                transform.Find("Child").gameObject.SetActive(false);
                GameManager.Instance.retryPanel.SetActive(true);
                //GameManager.Instance.clearPanel.SetActive(true);
                //GameManager.Instance.ClearSet();
            }
            else
            {
                Image child = transform.Find("Child").GetComponent<Image>();

                Color alpha = child.color;

                time += Time.deltaTime;

                if (alpha.a < 1f)
                {
                    childTime += Time.deltaTime * 2.5f;
                    alpha.a = Mathf.Lerp(0, 1, childTime);
                    child.color = alpha;
                }

                if (width >= 30f)
                    width -= Time.deltaTime * widthFadeSpeed * 3f;

                if (height >= 15f)
                    height -= Time.deltaTime * heightFadeSpeed * 3f;

                img.rectTransform.sizeDelta = new Vector2(width, height);
            }
        }
    }

    public void Fade()
    {
        isFade = true;

        time = 0;
        childTime = 0f;
    }

    public void OverFade()
    {
        isOverFade = true;

        time = 0;
        childTime = 0f;
    }
}