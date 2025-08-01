using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int deathCount;

    public bool[] isClears = new bool[3];

    public float bgm;
    public float sfx;

    public double playTime;
    public DateTime saveTime;
    public string saveTimeStr;
}