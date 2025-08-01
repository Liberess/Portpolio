using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour
{
    private const string GameDataFileName = "/GameData.json";
    //private const string PlayerDataFileName = "/PlayerData.json";

    #region 인스턴스화
    private static DataManager m_Instance;
    public static DataManager Instance
    {
        get
        {
            if (!m_Instance)
            {
                m_Container = new GameObject();
                m_Container.name = "DataManager";
                m_Instance = m_Container.AddComponent(
                    typeof(DataManager)) as DataManager;
                DontDestroyOnLoad(m_Container);
            }

            return m_Instance;
        }
    }

    private static GameObject m_Container;

    [Space(5), Header("==== Game Data Information ====")]
    [SerializeField] private GameData m_GameData;
    public GameData GameData
    {
        get
        {
            if (m_GameData == null)
                LoadGameData();

            return m_GameData;
        }
    }
    #endregion

    private void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (m_Instance != this)
        {
            Destroy(gameObject);
        }

        LoadGameData();
    }

    private void Start()
    {
        SaveGameData();
    }

    #region Game Data Load & Save
    public void InitializedGameData()
    {
        m_GameData.deathCount = 0;

        for (int i = 0; i < m_GameData.isClears.Length; i++)
            m_GameData.isClears[i] = false;
        
        m_GameData.sfx = 1.0f;
        m_GameData.bgm = 1.0f;

        m_GameData.playTime = 0.0f;
        m_GameData.saveTime = DateTime.Now;
    }

    public void OnClickNewGame()
    {
        InitializedGameData();
        SceneManager.LoadScene("Lobby");
    }

    public void OnClickContinueGame()
    {
        SceneManager.LoadScene("Lobby");
    }

    private void LoadGameData()
    {
        string filePath = Application.persistentDataPath + GameDataFileName;

        if (File.Exists(filePath))
        {
            string code = File.ReadAllText(filePath);
            byte[] bytes = Convert.FromBase64String(code);
            string FromJsonData = System.Text.Encoding.UTF8.GetString(bytes);
            m_GameData = JsonUtility.FromJson<GameData>(FromJsonData);
        }
        else
        {
            m_GameData = new GameData();
            File.Create(Application.persistentDataPath + GameDataFileName);

            InitializedGameData();
        }
    }

    private void SaveGameData()
    {
        string filePath = Application.persistentDataPath + GameDataFileName;

        string ToJsonData = JsonUtility.ToJson(m_GameData);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(ToJsonData);
        string code = Convert.ToBase64String(bytes);
        File.WriteAllText(filePath, code);
    }

    private void SaveTime()
    {
        var currentSaveTime = m_GameData.saveTime;
        var nowSaveTime = DateTime.Now;
        if (currentSaveTime != nowSaveTime)
        {
            var playTime = currentSaveTime - nowSaveTime;
            m_GameData.playTime += playTime.TotalSeconds;
        }

        m_GameData.saveTime = nowSaveTime;
        m_GameData.saveTimeStr = m_GameData.saveTime.ToString();
    }
    #endregion

    private void OnApplicationQuit()
    {
        SaveTime();
        SaveGameData();
    }
}