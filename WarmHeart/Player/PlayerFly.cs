using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFly : MonoBehaviour
{
    Rigidbody2D rigid;
    [SerializeField, Range(0f, 5f)] private float Flyforce;
    [SerializeField, Range(0f, 100f)] public float FlyPower;
    public int FlyCount=0;

    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (FlyCount == 0 && FlyPower < 100)
        {
            FlyPower = FlyPower + Time.deltaTime >= 100 ? 100 : FlyPower + Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            if (rigid.gravityScale == 0.3f)
                rigid.gravityScale = 1;
            if (FlyPower - Time.deltaTime * 3 >= 0)
            {
                FlyPower -= Time.deltaTime * 3;
                rigid.AddForce(Vector2.up, ForceMode2D.Impulse);
                if (rigid.velocity.y > Flyforce)//¿ÞÂÊ
                {
                    rigid.velocity = new Vector2(rigid.velocity.x, Flyforce );
                }
            }
        }
        if (Input.GetKey(KeyCode.D))
        {
            rigid.gravityScale = 0.3f;
        }

    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            FlyCount = 0;
            anim.SetBool("isFly", false);
            rigid.gravityScale = 1f;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            anim.SetBool("isFly", true);
            FlyCount = 1;
        }
    }
}
