using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public PlayerFly PlayerFly { get; private set; }
    private PlayerControl playerControl;

    public GameObject Fireball;
    [SerializeField] private Transform shotPos;

    private const float MinTimeBetAttack = 0.5f;
    private float lastAttackTime;

    protected bool IsAttack
    {
        get
        {
            if (Time.time >= lastAttackTime + MinTimeBetAttack)
                return true;

            return false;
        }
    }

    private void Start()
    {
        PlayerFly = GetComponent<PlayerFly>();
        playerControl = GetComponent<PlayerControl>();

        if (!shotPos)
            shotPos = transform.Find("ShotPos");
    }

    private void OnAttack()
    {
        if (IsAttack)
        { 
            if (PlayerFly.FlyPower - 5 >= 0)
            {
                lastAttackTime = Time.time;
                PlayerFly.FlyPower -= 5;

                var bullet = Instantiate(Fireball, shotPos.position, Quaternion.identity)
                    .GetComponent<FireBall>();

                DamageMessage dmg;
                dmg.damager = gameObject;
                dmg.damageAmount = 10;
                dmg.hitPoint = transform.position;

                DirectionType dirc = (playerControl.PlayerMove.XDirection > 0) ? DirectionType.Right : DirectionType.Left;
                bullet.SetupBullet(dmg, dirc);
            }
        }
    }
}
