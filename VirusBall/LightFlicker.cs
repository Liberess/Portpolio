using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LightFlicker : MonoBehaviour
{
    public UnityEngine.Experimental.Rendering.Universal.Light2D myLight;

    private float time = 0f;
    private float fadeTime = 0.2f;

    private float lightIntensity = 0.7f;

    private void Start()
    {
        StartCoroutine(FadeFlow());
    }

    IEnumerator FadeFlow()
    {
        time = 0f;

        while (myLight.intensity < lightIntensity)
        {
            time += Time.deltaTime / fadeTime;
            myLight.intensity = Mathf.Lerp(0, 1, time);
            yield return null;
        }

        time = 0f;

        yield return new WaitForSeconds(1f);

        while (myLight.intensity > 0f)
        {
            time += Time.deltaTime / fadeTime;
            myLight.intensity = Mathf.Lerp(1, 0, time);
            yield return null;
        }

        StartCoroutine(FadeFlow());
        //yield return null;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}