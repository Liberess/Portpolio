using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataManager : MonoBehaviour
{
    private string GameDataFileName = "/GameData.json";

    #region 인스턴스화
    static GameObject _container;
    static GameObject Container
    {
        get
        {
            return _container;
        }
    }

    public static DataManager _instance;
    public static DataManager Instance
    {
        get
        {
            if (!_instance)
            {
                _container = new GameObject();
                _container.name = "DataManager";
                _instance = _container.AddComponent(typeof(DataManager)) as DataManager;
                DontDestroyOnLoad(_container);
            }

            return _instance;
        }
    }

    public GameData _gameData;
    public GameData gameData
    {
        get
        {
            if (_gameData == null)
            {
                LoadGameData();
                SaveGameData();
            }

            return _gameData;
        }
    }
    #endregion

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        LoadGameData();
    }

    private void Start()
    {
        //ResetGame();
        SaveGameData();
    }

    public void ResetGame()
    {
        for (int i = 0; i < gameData.files.Length; i++)
        {
            gameData.isNew[i] = true;
            gameData.isClear[i] = false;
            gameData.files[i] = 0;
            gameData.clearTime[i] = 0;

            if(i < 3)
            {
                gameData.skillUnlock[i] = false;
            }
        }

        gameData.sfx = 1f;
        gameData.bgm = 1f;
    }

    #region Game Load & Save
    public void LoadGameData()
    {
        string filePath = Application.persistentDataPath + GameDataFileName;

        if (File.Exists(filePath))
        {
            string code = File.ReadAllText(filePath);
            byte[] bytes = System.Convert.FromBase64String(code);
            string FromJsonData = System.Text.Encoding.UTF8.GetString(bytes);
            _gameData = JsonUtility.FromJson<GameData>(FromJsonData);
        }
        else
        {
            _gameData = new GameData();
            File.Create(Application.persistentDataPath + GameDataFileName);

            for(int i = 0; i < gameData.files.Length; i++)
            {
                gameData.isNew[i] = true;
                gameData.files[i] = 0;
                gameData.clearTime[i] = 0;
            }

            gameData.sfx = 1f;
            gameData.bgm = 1f;
        }
    }

    public void SaveGameData()
    {
        string filePath = Application.persistentDataPath + GameDataFileName;

        string ToJsonData = JsonUtility.ToJson(gameData);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(ToJsonData);
        string code = System.Convert.ToBase64String(bytes);
        File.WriteAllText(filePath, code);
    }
    #endregion

    private void OnApplicationPause(bool pause)
    {
        SaveGameData();
    }

    private void OnApplicationQuit()
    {
        SaveGameData();
    }
}