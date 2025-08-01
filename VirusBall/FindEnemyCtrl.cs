using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindEnemyCtrl : Enemy
{
    Animator anim;
    SpriteRenderer sprite;

    private void Start()
    {
        moveSpeed = 2f;

        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        Think();
    }

    private void FixedUpdate()
    {
        Move();
    }

    #region 움직임
    protected override void Move()
    {
        if(!isWarning)
        {
            anim.SetBool("isFind", false);

            //transform.position = Vector2.MoveTowards(transform.position, desPos.position, Time.deltaTime * moveSpeed);

            Transform target = FindObjectOfType<Player>().transform;
            
            if(target.position.x > transform.position.x)
            {
                sprite.flipX = false;
            }
            else
            {
                sprite.flipX = true;
            }
        }
    }

    protected override void Turn()
    {
        CancelInvoke();

        if (isPlatformY) nextMoveY *= (-1);
        if (isPlatformX) nextMoveX *= (-1);

        Invoke("Think", 2f);
    }
    #endregion

    private void FindOff()
    {
        isWarning = false;
        anim.SetBool("isFind", false);
    }

    #region 물리 충돌
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        if(player != null)
        {
            if(!player.isHide)
            {
                isWarning = true;
                anim.SetBool("isFind", true);
                CancelInvoke("FindOff");
                SoundManager.Instance.PlaySFX("Warning");

                if(transform.position.x >= player.transform.position.x) //Flip X
                {
                    sprite.flipX = false;
                }
                else
                {
                    sprite.flipX = true;
                }

                EnemyManager.Instance.StraightOn();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        if (player != null)
        {
            if (player.isHide)
            {
                Invoke("FindOff", 1f);
                EnemyManager.Instance.StraightOff();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        if (player != null)
        {
            if (!player.isHide)
            {
                Invoke("FindOff", 1f);
                EnemyManager.Instance.StraightOff();
            }
        }

        /* if (collision.CompareTag("Player"))
        {
            isWarning = false;
            //EnemyManager.Instance.StraightOff();
        } */
    }
    #endregion
}