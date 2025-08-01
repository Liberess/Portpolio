using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bar : MonoBehaviour
{
    private int speed;
    private int buildIndex;

    public Vector2 leftPos;
    public Vector2 rightPos;

    private RectTransform rectPos;
    public RectTransform desPos;

    private void Awake()
    {
        buildIndex = SceneManager.GetActiveScene().buildIndex;

        switch(buildIndex)
        {
            case 2: speed = 80; break; //Level_1
            case 3: speed = 90; break;
            case 4: speed = 100; break;
            case 5: speed = 120; break;
            case 6: speed = 150; break;
        }

        rectPos = GetComponent<RectTransform>();
    }

    private void Start()
    {
        leftPos = rectPos.anchoredPosition;
        rightPos = new Vector2(rectPos.anchoredPosition.x + 68, rectPos.anchoredPosition.y);
        desPos.anchoredPosition = rightPos;
    }

    private void Update()
    {
        rectPos.anchoredPosition = Vector2.MoveTowards(rectPos.anchoredPosition, desPos.anchoredPosition, Time.deltaTime * speed);

        if (Vector2.Distance(rectPos.anchoredPosition, desPos.anchoredPosition) <= 0.05f)
        {
            if (desPos.anchoredPosition == rightPos)
            {
                desPos.anchoredPosition = leftPos;
            }
            else
            {
                desPos.anchoredPosition = rightPos;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Goal"))
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GameManager.Instance.isGoal = true;
            }
        }
    }
}