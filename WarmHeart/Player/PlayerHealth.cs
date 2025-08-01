using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : LivingEntity
{
    private ColoredFlash coloredFlash;

    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        coloredFlash = GetComponent<ColoredFlash>();

        minTimeBetDamaged = 1.5f;
    }

    public override void ApplyDamage(DamageMessage dmgMsg)
    {
        if (IsDamageable)
        {
            coloredFlash.Duration = minTimeBetDamaged;
            coloredFlash.Flash();
        }
        
        base.ApplyDamage(dmgMsg);
    }
}
