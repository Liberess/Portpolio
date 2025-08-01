using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

enum StarIndex
{
    Empty = 0,
    One,
    Two,
    Three
}

enum ClearIndex
{
    Gray = 0,
    Brown,
    Orange
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    private DataManager dataMgr;

    public Button[] levels;
    public GameObject[] lockImgs;
    public Image[] starImgs;
    public Sprite[] starSprites;
    public Text[] timeTxts;

    public Image[] stageImgs;
    public Sprite[] panelSprites;

    private void Awake()
    {
        Instance = this;
        dataMgr = DataManager.Instance;
    }

    private void Start()
    {
        SetTimes();
        SetBtns();
        SetStars();
    }

    private void SetTimes()
    {
        //Set Clear Times
        for (int i = 0; i < DataManager.Instance.gameData.clearTime.Length; i++)
        {
            if(DataManager.Instance.gameData.clearTime[i] <= 0)
            {
                timeTxts[i].gameObject.SetActive(false);
            }
            else
            {
                float time = DataManager.Instance.gameData.clearTime[i];
                int minute = (int)time / 60;
                int second = (int)time % 60;
                minute %= 60;

                timeTxts[i].text = minute + ":" + second;
            }
        }
    }

    private void SetBtns()
    {
        //Set Button Interaction
        for (int i = 0; i < 5; i++)
        {
            if(i == 0)
            {
                levels[i].interactable = true;
                continue;
            }

            if (dataMgr.gameData.isClear[i - 1])
            {
                levels[i].interactable = true;
                lockImgs[i - 1].SetActive(false);
            }
            else
            {
                levels[i].interactable = false;
            }
        }
    }    

    private void SetStars()
    {
        for (int i = 0; i < dataMgr.gameData.isClear.Length; i++)
        {
            if(dataMgr.gameData.clearTime[i] <= 0)
            {
                starImgs[i].sprite = starSprites[(int)StarIndex.Empty];
                stageImgs[i].sprite = panelSprites[(int)ClearIndex.Gray];
                continue;
            }

            if (dataMgr.gameData.files[i] >= dataMgr.gameData.maxFiles[i]
                && dataMgr.gameData.clearTime[i] <= dataMgr.gameData.missionTime[i])
            {
                starImgs[i].sprite = starSprites[(int)StarIndex.Three];
                stageImgs[i].sprite = panelSprites[(int)ClearIndex.Orange];
            }
            else if(dataMgr.gameData.files[i] < dataMgr.gameData.maxFiles[i]
                && dataMgr.gameData.clearTime[i] > dataMgr.gameData.missionTime[i])
            {
                starImgs[i].sprite = starSprites[(int)StarIndex.One];
                stageImgs[i].sprite = panelSprites[(int)ClearIndex.Brown];
            }
            else
            {
                starImgs[i].sprite = starSprites[(int)StarIndex.Two];
                stageImgs[i].sprite = panelSprites[(int)ClearIndex.Brown];
            }
        }
    }

    public void ReturnMain() => SceneManager.LoadScene("Main");
}