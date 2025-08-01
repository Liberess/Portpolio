using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    Action playerChaseOn;
    Action playerChaseOff;

    [Header ("먹는 백신들") ]
    public EatEnemyCtrl[] eatEnemies;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        eatEnemies = FindObjectsOfType<EatEnemyCtrl>();

        for (int i = 0; i < eatEnemies.Length; i++)
        {
            playerChaseOn += eatEnemies[i].Straight;
            playerChaseOff += eatEnemies[i].StraightOff;
        }
    }

    public void StraightOn()
    {
        playerChaseOn();
    }

    public void StraightOff()
    {
        playerChaseOff();
    }
}