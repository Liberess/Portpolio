using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainEnemy : MonoBehaviour
{
    //private float moveSpeed = 5f;

    Animator anim;
    SpriteRenderer sprite;

    private void Start()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        anim.SetBool("isFind", true);
    }

    private void FixedUpdate()
    {
        Transform target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        //transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * 1.5f * Time.deltaTime);

        if (transform.position.x >= target.transform.position.x) //Flip X
        {
            sprite.flipX = false;
        }
        else
        {
            sprite.flipX = true;
        }
    }
}