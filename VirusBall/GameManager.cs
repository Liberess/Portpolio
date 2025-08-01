using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private DataManager dataMgr;

    public Text timeTxt;
    public Text fileTxt;

    public GameObject retryPanel;
    public GameObject menuPanel;
    public GameObject clearPanel;

    public Sprite[] starSprites;

    public bool isPlay;
    private bool isGameOver;
    public int buildIndex;

    private int file;

    private int minute = 0;
    private int second = 0;
    private float playTime = 0f;

    public bool isGoal;
    public bool isMission;

    Action gameOver;

    private void Awake()
    {
        Instance = this;

        buildIndex = SceneManager.GetActiveScene().buildIndex;

        if (buildIndex > 1) //1은 Stage
        {
            retryPanel.SetActive(false);
            menuPanel.SetActive(false);
        }
    }

    private void Start()
    {
        dataMgr = DataManager.Instance;

        if (buildIndex > 1)
        {
            file = 0;
            //isStop = false;
            isPlay = true;
            isGameOver = false;

            gameOver += Player.Instance.Die;
        }

        SoundManager.Instance.PlayBGM(SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        if (buildIndex > 1)
        {
            SetTxt(); //시간, 파일 텍스트 동기화
            MenuCtrl(); //메뉴창 띄우기

            if (clearPanel.activeSelf && Input.GetButtonDown("Submit"))
            {
                SceneManager.LoadScene("Stage");
            }
        }

        if (buildIndex <= 1)
        {
            if (Input.GetButtonDown("Cancel"))
            {
                SoundManager.Instance.PlaySFX("UIClick");
                Option();
            }
        }

        HiddenKey();
    }

    private void HiddenKey()
    {
        if (Input.GetKeyDown(KeyCode.F12))
            dataMgr.ResetGame();

        if (Input.GetKeyDown(KeyCode.F11))
        {
            for (int i = 0; i < dataMgr.gameData.skillUnlock.Length; i++)
            {
                dataMgr.gameData.skillUnlock[i] = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            for (int i = 0; i < dataMgr.gameData.cool.Length; i++)
            {
                dataMgr.gameData.cool[i] = 1;
            }
        }

        if (Input.GetKeyDown(KeyCode.F1))
            SceneManager.LoadScene("Main");

        if (Input.GetKeyDown(KeyCode.F2))
            SceneManager.LoadScene("Stage");

        if (Input.GetKeyDown(KeyCode.F3))
            SceneManager.LoadScene("Level_1");

        if (Input.GetKeyDown(KeyCode.F4))
            SceneManager.LoadScene("Level_2");

        if (Input.GetKeyDown(KeyCode.F5))
            SceneManager.LoadScene("Level_3");

        if (Input.GetKeyDown(KeyCode.F6))
            SceneManager.LoadScene("Level_4");

        if (Input.GetKeyDown(KeyCode.F7))
            SceneManager.LoadScene("Level_5");
    }

    private void MenuCtrl()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            SoundManager.Instance.PlaySFX("UIClick");

            if (menuPanel.activeSelf)
            {
                menuPanel.SetActive(false);
                //Time.timeScale = 1;
            }
            else
            {
                menuPanel.SetActive(true);
                //Time.timeScale = 0;
            }
        }
    }

    public void SetTxt()
    {
        if (!isGameOver) //게임이 Over되지 않았을 때
        {
            playTime += Time.deltaTime;
            minute = (int)playTime / 60;
            second = (int)playTime % 60;
            minute %= 60;

            timeTxt.text = string.Format("{0:D2}", minute) + ":" + string.Format("{0:D2}", second);
            fileTxt.text = "파일 : " + file + "/" + dataMgr.gameData.maxFiles[buildIndex - 2];
        }
    }

    public void AddFile()
    {
        if (file <= dataMgr.gameData.maxFiles[buildIndex - 2])
        {
            file++;        
        }
    }

    public void Option()
    {
        SoundManager.Instance.PlayBGM("UIClick");

        if (menuPanel.activeSelf)
        {
            //Time.timeScale = 1;
            menuPanel.SetActive(false);
        }
        else
        {
            //Time.timeScale = 0;
            menuPanel.SetActive(true);
        }
    }

    /* public void TotalSet()
    {
        float time = dataMgr.gameData.totalPlayTime;

        int minute = (int)time / 60;
        int hour = minute / 60;
        int second = (int)time % 60;
        minute %= 60;

        GameObject.Find("TotalTime").GetComponent<Text>().text =
            hour + "시간 " + minute + "분 " + second + "초";
    } */

    #region Stage Clear
    public void StageClear()
    {
        isPlay = false;
        SoundManager.Instance.PlayBGM("GameClear");

        GameObject fadeObj = GameObject.Find("GameCanvas").transform.Find("Fade").gameObject;
        fadeObj.GetComponent<Image>().enabled = true;
        fadeObj.GetComponent<CircleFade>().Fade();

        if (dataMgr.gameData.isNew[buildIndex - 2]) //데이터 덮어쓰기
        {
            dataMgr.gameData.isClear[buildIndex - 2] = true;
            dataMgr.gameData.isNew[buildIndex - 2] = false;
            dataMgr.gameData.clearTime[buildIndex - 2] = playTime;
            dataMgr.gameData.files[buildIndex - 2] = file;
        }
        else
        {
            if (playTime <= dataMgr.gameData.clearTime[buildIndex - 2])
            {
                dataMgr.gameData.clearTime[buildIndex - 2] = playTime;
            }

            if (file >= dataMgr.gameData.files[buildIndex - 2])
            {
                dataMgr.gameData.files[buildIndex - 2] = file;
            }
        }
    }

    public void ClearSet()
    {
        Image starImg = GameObject.Find("ClearChildPanel").transform.Find("StarImg").GetComponent<Image>();
        Image fileImg = GameObject.Find("ClearChildPanel").transform.Find("FileImg").GetComponent<Image>();
        Image timeImg = GameObject.Find("ClearChildPanel").transform.Find("TimeImg").GetComponent<Image>();

        Text fileBadTxt = GameObject.Find("ClearChildPanel").transform.Find("FileBadTxt").GetComponent<Text>();
        Text timeBadTxt = GameObject.Find("ClearChildPanel").transform.Find("TimeBadTxt").GetComponent<Text>();

        Text fileGoodTxt = GameObject.Find("ClearChildPanel").transform.Find("FileGoodTxt").GetComponent<Text>();
        Text timeGoodTxt = GameObject.Find("ClearChildPanel").transform.Find("TimeGoodTxt").GetComponent<Text>();

        if ((file >= dataMgr.gameData.maxFiles[buildIndex - 2])
            && (playTime <= dataMgr.gameData.missionTime[buildIndex - 2]))
        {
            starImg.sprite = starSprites[(int)StarIndex.Three];
            fileImg.sprite = Resources.Load<Sprite>("O");
            timeImg.sprite = Resources.Load<Sprite>("O");

            fileGoodTxt.gameObject.SetActive(true);
            timeGoodTxt.gameObject.SetActive(true);
        }
        else
        {
            if ((file >= dataMgr.gameData.maxFiles[buildIndex - 2])
                && (playTime > dataMgr.gameData.missionTime[buildIndex - 2]))
            {
                starImg.sprite = starSprites[(int)StarIndex.Two];
                fileImg.sprite = Resources.Load<Sprite>("O");
                timeImg.sprite = Resources.Load<Sprite>("X");

                fileGoodTxt.gameObject.SetActive(true);
                timeBadTxt.gameObject.SetActive(true);
            }
            else if ((file < dataMgr.gameData.maxFiles[buildIndex - 2])
                && (playTime <= dataMgr.gameData.missionTime[buildIndex - 2]))
            {
                starImg.sprite = starSprites[(int)StarIndex.Two];
                fileImg.sprite = Resources.Load<Sprite>("X");
                timeImg.sprite = Resources.Load<Sprite>("O");

                fileBadTxt.gameObject.SetActive(true);
                timeGoodTxt.gameObject.SetActive(true);
            }
            else
            {
                starImg.sprite = starSprites[(int)StarIndex.One];
                fileImg.sprite = Resources.Load<Sprite>("X");
                timeImg.sprite = Resources.Load<Sprite>("X");

                fileBadTxt.gameObject.SetActive(true);
                timeBadTxt.gameObject.SetActive(true);
            }
        }

        if (fileGoodTxt.gameObject.activeSelf)
            fileGoodTxt.text = "파일 " + dataMgr.gameData.maxFiles[buildIndex - 2] + "개 획득";
        else
            fileBadTxt.text = "파일 " + dataMgr.gameData.maxFiles[buildIndex - 2] + "개 획득";

        if (timeGoodTxt.gameObject.activeSelf)
            timeGoodTxt.text = TimeUnitChange();
        else
            timeBadTxt.text = TimeUnitChange();
    }

    private string TimeUnitChange()
    {
        float time = dataMgr.gameData.missionTime[buildIndex - 2];

        minute = (int)time / 60;
        second = (int)time % 60;
        minute %= 60;

        string timeTxt = minute + "분 " + second + "초 이내 클리어";

        return timeTxt;
    }
    #endregion

    #region Game Control
    public void GameOver()
    {
        isPlay = false;
        isGameOver = true;

        SoundManager.Instance.PlayBGM("GameOver");

        GameObject fadeObj = GameObject.Find("GameCanvas").transform.Find("Fade").gameObject;
        fadeObj.GetComponent<Image>().enabled = true;
        fadeObj.GetComponent<CircleFade>().OverFade();

        //retryPanel.SetActive(true);
        gameOver();
    }

    public void GameRetry()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(buildIndex);
    }

    public void GameQuit()
    {
        /* if (playTime > dataMgr.gameData.clearTime[buildIndex - 2])
        {
            dataMgr.gameData.clearTime[buildIndex - 2] = playTime;
        } */
        Application.Quit();
    }
    #endregion
}