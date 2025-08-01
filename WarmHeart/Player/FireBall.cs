using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : Bullet
{
    private void Start()
    {
        Destroy(gameObject, destoryTime);
    }

    protected override void OnEnter(Collider2D collider)
    {
        if (!collider.CompareTag("Player"))
        {
            if (collider.TryGetComponent(out LivingEntity livingEntity))
                livingEntity.ApplyDamage(dmgMsg);

            if (collider.TryGetComponent(out StageKeyDoor door))
                door.OnEnter();
        }

        Destroy(gameObject);
    }
}
