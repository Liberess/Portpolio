using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindBlow: MonoBehaviour
{
    public int Direction;
    [SerializeField] float WindPower = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && collision.GetComponent<PlayerFly>().FlyCount == 1)
        {
            switch (Direction)
            {
                case 0:
                    collision.GetComponent<Rigidbody2D>().AddForce(Vector2.up * WindPower , ForceMode2D.Impulse);
                    break;
                case 1:
                    collision.GetComponent<Rigidbody2D>().AddForce(Vector2.down * WindPower , ForceMode2D.Impulse);
                    break;
                case 2:
                    collision.GetComponent<Rigidbody2D>().AddForce(Vector2.right * WindPower, ForceMode2D.Impulse);
                    break;
                case 3:
                    collision.GetComponent<Rigidbody2D>().AddForce(Vector2.left * WindPower, ForceMode2D.Impulse);
                    break;

            }
            
        }
    }
}
