using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TypingText : MonoBehaviour
{
    [SerializeField] private Text typingTxt;
    [SerializeField, TextArea] private List<string> txtList = new List<string>();

    [SerializeField, Range(0f, 5f)] private float typingStartTime = 2f;
    [SerializeField, Range(0f, 5f)] private float typingSpeed = 0.1f;
    [SerializeField, Range(0f, 5f)] private float nextTypingDelayTime = 2f;

    private void Start()
    {
        StartCoroutine(TypingCo());
    }

    private IEnumerator TypingCo()
    {
        yield return new WaitForSeconds(typingStartTime);

        for(int i = 0; i < txtList.Count; i++)
        {
            for (int j = 0; j <= txtList[i].Length; j++)
            {
                typingTxt.text = txtList[i].Substring(0, j);
                yield return new WaitForSeconds(typingSpeed);
            }

            yield return new WaitForSeconds(nextTypingDelayTime);
        }

        yield return null;
    }
}