using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Fly = 0, //A
    Hide, //S
    Flicker, //D
}

[System.Serializable]
public class GameData
{
    public float bgm;
    public float sfx;

    public bool[] isNew = new bool[5];

    public int[] cool = { 5, 10, 5 };
    public bool[] skillUnlock = new bool[3];

    public int[] files = new int[5];
    public int[] maxFiles = { 14, 17, 10, 20, 13 };

    public bool[] isClear = new bool[5];
    public float[] clearTime = new float[5];
    public float[] missionTime = { 90f, 180f, 180f, 180f, 90f };

    public int totalPlayTime;
    public int stagePlayTime;
}