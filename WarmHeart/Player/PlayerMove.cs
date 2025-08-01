using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    Rigidbody2D rigid;
    [SerializeField, Range(0f, 5f)] private float Movespeed;

    [SerializeField] private Vector2 inputValue;
    [SerializeField] private Vector2 moveVec;
    [SerializeField] float wind_maxspeed;

    public bool XFlip => (moveVec.x > 0) ? true : false;
    public int XDirection => (moveVec.x > 0) ? 1 : -1;

    private Animator anim;
    private SpriteRenderer sprite;
    float left_maxspeed;
    float right_maxspeed;
    private void Awake()
    {
        left_maxspeed = Movespeed;
        right_maxspeed = Movespeed;
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();

        moveVec.x = 1;
    }

    private void Update()
    {
        if(inputValue != Vector2.zero)
        {
            moveVec = new Vector2 (inputValue.x, inputValue.y);
            anim.SetBool("isWalk", true);

            sprite.flipX = XFlip;
        }
        else
        {
            anim.SetBool("isWalk", false);
        }
    }

    private void OnMove(InputValue inputValue)
    {
        this.inputValue = inputValue.Get<Vector2>();
    }
  

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rigid.AddForce(Vector2.left, ForceMode2D.Impulse);
            if (rigid.velocity.x < left_maxspeed * (-1))//왼쪽
            {
                rigid.velocity = new Vector2(left_maxspeed * (-1), rigid.velocity.y);
            }
            
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            rigid.AddForce(Vector2.right , ForceMode2D.Impulse);
            if (rigid.velocity.x > right_maxspeed)//오른쪽
            {
                rigid.velocity = new Vector2(right_maxspeed, rigid.velocity.y);//y값을 0으로 잡으면 공중에서 멈춰버림
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out WindBlow windblow))
        {
            switch (windblow.Direction)
            {
                case 2:
                    left_maxspeed -= wind_maxspeed;
                    right_maxspeed += wind_maxspeed;
                    break;
                case 3:
                    left_maxspeed += wind_maxspeed;
                    right_maxspeed -= wind_maxspeed;
                    break;
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        left_maxspeed = Movespeed;
        right_maxspeed = Movespeed;
    }

}
