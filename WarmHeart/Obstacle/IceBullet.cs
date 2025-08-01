using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBullet : Bullet
{
    protected override void OnEnter(Collider2D collider)
    {
        if (collider.TryGetComponent(out PlayerHealth playerHealth))
        {
            DamageMessage dmg;
            dmg.damager = gameObject;
            dmg.damageAmount = 10;
            dmg.hitPoint = transform.position;

            playerHealth.ApplyDamage(dmg);
        }

        Destroy(gameObject);
    }
}
