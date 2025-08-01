using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallFall : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float BallPower=2f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            int ball_direction = transform.position.x > collision.transform.position.x ? 1 : -1;
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up * BallPower,ForceMode2D.Impulse);
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.left * BallPower * ball_direction, ForceMode2D.Impulse);
            DamageMessage dmg;
            dmg.damager = gameObject;
            dmg.damageAmount = 10;
            dmg.hitPoint = transform.position;

            collision.gameObject.GetComponent<PlayerHealth>().ApplyDamage(dmg);
        }
    }
}
