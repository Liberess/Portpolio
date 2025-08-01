using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [SerializeField]
    protected bool isChase; //추적 중인가

    public bool isWarning; //경고음 떴는가
    protected bool isPlatformX; //플랫폼에 닿았는가
    protected bool isPlatformY; //플랫폼에 닿았는가

    protected bool isUp; //플랫폼에 닿았는가
    protected bool isDown; //플랫폼에 닿았는가
    protected bool isLeft; //플랫폼에 닿았는가
    protected bool isRight; //플랫폼에 닿았는가

    protected int nextMoveX; //다음 X좌표 이동 방향 (왼쪽, 오른쪽)
    protected int nextMoveY; //다음 Y좌표 이동 방향 (왼쪽, 오른쪽)

    protected float accel = 1.5f; //플레이어 추적 가속도
    protected float maxSpeed = 5f; //이동 속도
    protected float moveSpeed = 5f; //이동 속도

    protected Transform target;
    protected Vector2 direction;

    protected abstract void Move();
    protected abstract void Turn();

    protected void Think()
    {
        //다음 이동 방향
        nextMoveX = UnityEngine.Random.Range(0, 2);
        nextMoveY = UnityEngine.Random.Range(0, 2);

        if (nextMoveX == 0)
        {
            nextMoveX = -1;
        }

        if (nextMoveY == 0)
        {
            nextMoveY = -1;
        }

        //재귀 호출
        float nextThinkTime = UnityEngine.Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }
}