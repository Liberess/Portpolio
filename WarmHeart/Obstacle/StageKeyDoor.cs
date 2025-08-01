using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageKeyDoor : MonoBehaviour
{
    public void OnEnter()
    {
        if (GameManager.Instance.IsKey)
        {
            var sceneNum = GameManager.Instance.SceneName.Substring(6);
            DataManager.Instance.GameData.isClears[int.Parse(sceneNum)] = true;
        }
    }
}