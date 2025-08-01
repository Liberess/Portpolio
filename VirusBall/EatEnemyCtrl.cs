using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Directions
{
    Up = 0,
    Down,
    Left,
    Right
}

public class EatEnemyCtrl : Enemy
{
    private bool isBack; //원 위치

    private Vector2 originPos; //원 위치

    Animator anim;
    BoxCollider2D boxCol;

    private void Start()
    {
        isBack = false;

        moveSpeed = 3f;
        originPos = transform.position;

        anim = GetComponent<Animator>();
        boxCol = GetComponent<BoxCollider2D>();

        Think();
    }

    private void FixedUpdate()
    {
        Move();
    }

    #region 움직임
    protected override void Move()
    {
        if(GameManager.Instance.isPlay)
        {
            if (isChase == true && !isBack) //플레이어를 쫓는다면
            {
                CancelInvoke();

                target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

                //플레이어 방향으로 속도 * 가속도만큼 이동
                transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * accel * Time.deltaTime);

                isPlatformX = Physics2D.OverlapCircle(new Vector2(transform.position.x + (1f * nextMoveX), transform.position.y), 0.5f, LayerMask.GetMask("Platform"));
                isPlatformY = Physics2D.OverlapCircle(new Vector2(transform.position.x, transform.position.y + (1f * nextMoveY)), 0.5f, LayerMask.GetMask("Platform"));

                bool isPlatform = isPlatformX || isPlatformY;

                if (isPlatform) //만약 플랫폼에 닿았다면
                {
                    Turn();
                }
            }

            if (isBack == true)
            {
                boxCol.isTrigger = true;

                Vector2 nowPos = new Vector2(transform.position.x, transform.position.y);

                if (nowPos != originPos)
                {
                    transform.position = Vector2.MoveTowards(transform.position, originPos, moveSpeed * Time.deltaTime);
                }
                else
                {
                    isBack = false;
                    boxCol.isTrigger = false;
                }
            }
        }
    }

    protected override void Turn()
    {
        CancelInvoke();

        if(isPlatformY) nextMoveY *= (-1);
        if(isPlatformX) nextMoveX *= (-1);

        Invoke("Think", 2f);
    }

    public void Straight()
    {
        boxCol.isTrigger = true;
    }

    public void StraightOff()
    {
        boxCol.isTrigger = false;
    }
    #endregion

    #region 물리 충돌
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            if(isBack == false)
            {
                accel = 1.5f;
                isChase = true;
                anim.SetBool("isFind", true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            accel = 1f;
            isBack = true;
            isChase = false;
            anim.SetBool("isFind", false);
        }
    }
    #endregion
}