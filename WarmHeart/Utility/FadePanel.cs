using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadePanel : MonoBehaviour
{
    public static FadePanel Instance { get; private set; }

    [SerializeField] private Image panel;

    private float time = 0f;
    [SerializeField, Range(0.0f, 10.0f)] private float fadeTime = 0.2f;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    private void Start()
    {
        if(!panel)
            panel = GetComponentInChildren<Image>();
    }

    public void Fade() => StartCoroutine(FadeFlow());

    public void FadeIn() => StartCoroutine(FadeInProcess());

    public void FadeOut() => StartCoroutine(FadeOutProcess());

    private IEnumerator FadeInProcess()
    {
        panel.gameObject.SetActive(true);

        time = 0f;

        Color alpha = panel.color;

        while (alpha.a < 1f)
        {
            time += Time.deltaTime / fadeTime;
            alpha.a = Mathf.Lerp(0, 1, time);
            panel.color = alpha;
            yield return null;
        }
    }

    private IEnumerator FadeOutProcess()
    {
        panel.gameObject.SetActive(true);

        time = 0f;

        Color alpha = new Color(0f, 0f, 0f, 1f);

        while (alpha.a > 0f)
        {
            time += Time.deltaTime / fadeTime;
            alpha.a = Mathf.Lerp(1, 0, time);
            panel.color = alpha;
            yield return null;
        }

        panel.gameObject.SetActive(false);
        yield return null;
    }

    private IEnumerator FadeFlow()
    {
        panel.gameObject.SetActive(true);

        time = 0f;

        Color alpha = panel.color;

        while(alpha.a < 1f)
        {
            time += Time.deltaTime / fadeTime;
            alpha.a = Mathf.Lerp(0, 1, time);
            panel.color = alpha;
            yield return null;
        }

        time = 0f;

        yield return new WaitForSeconds(1f);

        while (alpha.a > 0f)
        {
            time += Time.deltaTime / fadeTime;
            alpha.a = Mathf.Lerp(1, 0, time);
            panel.color = alpha;
            yield return null;
        }

        panel.gameObject.SetActive(false);
        yield return null;
    }
}
