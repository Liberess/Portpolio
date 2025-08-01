using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TxtFade : MonoBehaviour
{
    private Text txt;

    private float time = 0f;
    private float fadeTime = 1f;

    private void Start()
    {
        txt = GetComponent<Text>();

        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        Color alpha = txt.color;

        time = 0f;

        while (alpha.a < 1f)
        {
            time += Time.deltaTime / fadeTime;
            alpha.a = Mathf.Lerp(0, 1, time);
            txt.color = alpha;
            yield return null;
        }

        time = 0f;

        yield return new WaitForSeconds(1f);

        while (alpha.a > 0f)
        {
            time += Time.deltaTime / fadeTime;
            alpha.a = Mathf.Lerp(1, 0, time);
            txt.color = alpha;
            yield return null;
        }

        //yield return null;
        StartCoroutine(Fade());
    }
}