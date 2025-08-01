using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;

public class Thorn : MonoBehaviour
{
    private bool isActive = false;

    [SerializeField] private float activeRange = 10.0f;
    private float targetDistance;
    private Transform target;

    private BoxCollider2D boxCol;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(Color.red.a, Color.red.g, Color.red.b, 0.5f);
        Gizmos.DrawSphere(transform.position, activeRange);
    }

    private void Awake()
    {
        boxCol = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        boxCol.enabled = false;
        target = FindObjectOfType<PlayerControl>().transform;
    }

    private void Update()
    {
        if (isActive)
            return;

        targetDistance = Vector2.Distance(transform.position, target.position);
        if(targetDistance <= activeRange)
        {
            isActive = true;
            gameObject.GetComponent<Rigidbody2D>().gravityScale = 1;
            StartCoroutine(SetBoxColliderEnabledCo(true, 1f));
        }
    }

    private IEnumerator SetBoxColliderEnabledCo(bool active, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        boxCol.enabled = active;
        yield return null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out PlayerHealth playerHealth))
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
