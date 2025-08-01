using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DamageMessage
{
    public GameObject damager;
    public int damageAmount;
    public Vector2 hitPoint;
}

public interface IDamageable
{
    void ApplyDamage(DamageMessage dmgMsg);
}