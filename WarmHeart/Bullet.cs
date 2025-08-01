using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Bullet : MonoBehaviour
{
    [SerializeField] protected float fireMoveSpeed = 3f;
    [SerializeField] protected float destoryTime = 10f;
    protected DirectionType dircType;
    protected DamageMessage dmgMsg;

    protected abstract void OnEnter(Collider2D collider);

    private void Start()
    {
        Destroy(gameObject, destoryTime);
    }

    private void FixedUpdate()
    {
        transform.Translate(Vector2.right * fireMoveSpeed * Time.deltaTime);
    }

    public void SetupBullet(DamageMessage _dmgMsg, DirectionType _dircType)
    {
        dmgMsg = _dmgMsg;
        dircType = _dircType;

        switch (dircType)
        {
            case DirectionType.Up:
                transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                break;
            case DirectionType.Down:
                transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                break;
            case DirectionType.Left:
                transform.rotation = Quaternion.Euler(0f, -180, 0f);
                transform.Translate(1, 0, 0);
                break;
            case DirectionType.Right:
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        OnEnter(collider);
    }
}