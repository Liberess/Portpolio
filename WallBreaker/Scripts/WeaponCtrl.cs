using System;
using System.Collections;
using System.Collections.Generic;
using Consts;
using UnityEngine;
using Random = UnityEngine.Random;

public class WeaponCtrl : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotSpeed = 4f;

    public eAttack type;

    private bool isMove = false;

    private Vector2 moveDir;

    [SerializeField] private Animator anim;
    [SerializeField] private Rigidbody2D rigid;
    [SerializeField] private SpriteRenderer spriteRen;

    private void FixedUpdate()
    {
        if(!isMove)
            return;

        switch (type)
        {
            case eAttack.Forward: UpdateForward(); break;
            case eAttack.Rotate: UpdateRotate(); break;
        }
    }

    public void Setup(Transform target, int weaponIdx, int type)
    {
        rigid.linearVelocity = Vector2.zero;
        rigid.angularVelocity = 0f;
        isMove = false;

        this.type = (eAttack)type;

        spriteRen.sprite = ResourceMgr.Inst.GetSprite($"{Path.WeaponSprite}{weaponIdx}");

        var animCtrl = ResourceMgr.Inst.GetAnimator($"{Path.WeaponAnimator}{weaponIdx}");
        if(animCtrl != null)
            anim.runtimeAnimatorController = animCtrl;

        float newY = transform.position.y + Random.Range(-1f, 1f);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        moveDir = (target.position - transform.position).normalized;
        float rotZ = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rotZ);

        isMove = true;
    }

    private void UpdateForward()
    {
        rigid.linearVelocity = moveDir * moveSpeed;
    }

    private void UpdateRotate()
    {
        rigid.linearVelocity = moveDir * moveSpeed;
        transform.GetChild(0).Rotate(0, 0, -rotSpeed);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
            Destroy(gameObject, 0.01f);
    }
}
