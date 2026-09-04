using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using CognitioConsulting.Numerics;
using Consts;
using DG.Tweening;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public abstract class Breakable : MonoBehaviour
{
    protected BigDecimal maxHp = 100;
    public BigDecimal MaxHp => maxHp;
    public void SetMaxHp(BigDecimal amount) => maxHp = amount;
    
    protected BigDecimal hp;
    
    public BigDecimal Hp
    {
        get => hp;
        set
        {
            hp = value;

            if (hp > maxHp)
                hp = maxHp;
            else if(hp <= 0)
                OnBreak();
        }
    }

    protected eWall wallKind;
    protected eCategory_Wall category;

    protected bool isAlive = true;
    public bool IsAlive => isAlive;

    protected string spritePath;
    
    [SerializeField, Range(0.1f, 5f)] private float wallVelocity = 2f;

    private Transform moveEndTs;

    public Action OnBreakAction;

    private SpriteRenderer spriteRd;
    
    protected abstract void Init();

    private void Awake()
    {
        spriteRd = GetComponentInChildren<SpriteRenderer>();
        
        Init();
    }

    public virtual void Setup(eWall wallKind, eCategory_Wall category, Transform target, string spritePath)
    {
        this.wallKind = wallKind;
        this.category = category;
        this.spritePath = spritePath;
        moveEndTs = target;
        
        hp = maxHp;
        UIMgr.Inst.OnUpdateWallSlider(hp, maxHp);
        
        spriteRd.sprite = Util.GetSprite(spritePath, 0);
        
        Dispatcher.UpdateFrame(() =>
        {
            if (this == null || !isAlive)
                return true;
            
            transform.position = Vector3.Lerp(transform.position, moveEndTs.position,
                wallVelocity * Time.deltaTime);
            
            if (Vector3.Distance(transform.position, moveEndTs.position) <= 0.1f)
            {
                BreakableSpawner.Inst.SetBreakable(true);
                transform.position = moveEndTs.position;
            }
            
            return false;
        }, Time.fixedDeltaTime);
    }

    public void ApplyDamage(BigDecimal damage)
    {
        Hp -= damage;

        if (!isAlive || hp <= 0)
            return;

        transform.DOKill(false);
        transform.DOShakePosition(0.3f, 0.2f, 5, 90, false, true)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

        var eftType = eEffect.Hit_Wall_Normal;
        if(wallKind == eWall.Bonus_Gold)
            eftType = eEffect.Hit_Wall_Bonus;

        var eft = EffectMgr.Inst.InstantiateObj(eftType);
        eft.transform.SetParent(UIMgr.Inst.EffectRoot);
        eft.transform.position = Util.GetWorldToUISpace(UIMgr.Inst.EffectRoot.root.GetComponent<Canvas>(), transform.position);
        eft.transform.localScale = Vector3.one * 2f;
        EffectMgr.Inst.ReturnObjByDelay(eftType, eft, 0.5f);
        //eft.transform.localScale = Vector3.one * 0.5f;

        if(isAlive)
            UIMgr.Inst.OnUpdateWallSlider(hp, maxHp);
        
        float ratio = (float)(BigDecimal.Divide(hp, maxHp));
        
        if (ratio >= 0.65f)
            spriteRd.sprite = Util.GetSprite(spritePath, 0);
        else if (ratio >= 0.3f)
            spriteRd.sprite = Util.GetSprite(spritePath, 1);
        else if (ratio >= 0f)
            spriteRd.sprite = Util.GetSprite(spritePath, 2);
    }

    protected virtual void OnBreak()
    {
        if (!isAlive)
            return;

        transform.DOKill(true);
        DOTween.Kill(gameObject, true);

        hp = 0;
        isAlive = false;
        UIMgr.Inst.OnUpdateWallSlider(hp, maxHp);

        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = UnityEngine.Vector2.zero;
            rb.simulated = false;
        }

        if (TryGetComponent<Collider2D>(out var col))
            col.enabled = false;

        if (spriteRd != null)
            spriteRd.enabled = false;
        
        SoundMgr.Inst.PlaySfx(eSfxCategory.Wall, category.ToString());
        SoundMgr.Inst.PlaySfx(eSfxCategory.UI, "Coins", 0.2f).Forget();
        OnBreakAction?.Invoke();
        Destroy(gameObject);
    }
}
