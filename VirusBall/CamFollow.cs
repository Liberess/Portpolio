using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    [SerializeField]
    private float camSpeed = 5.0f;

    [SerializeField]
    private Transform target;

    private void Start()
    {
        target = FindObjectOfType<Player>().transform;
    }

    private void FixedUpdate()
    {
        Vector2 dir = target.position - this.transform.position;
        Vector2 moveVec = new Vector2(dir.x * camSpeed * Time.deltaTime, dir.y * camSpeed * Time.deltaTime);
        this.transform.Translate(moveVec);
    }

    private void LateUpdate()
    {

    }
}