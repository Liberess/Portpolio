using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    [SerializeField]
    private Transform mouse;

    [SerializeField]
    public float moveSpeed = 2f;

    private float time = 0;
    private float delayTime = 2f;

    private void Start()
    {
        Shake();
    }

    private void Update()
    {
        if(time >= delayTime)
        {
            time = 0;

            GameObject infection =  Instantiate(Resources.Load<GameObject>("Particles/Infection"), mouse.position, Quaternion.identity);
            Destroy(infection, 5f);
        }
        else
        {
            time += Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        transform.Translate(Vector2.right * Time.deltaTime * moveSpeed);
    }

    private void Shake()
    {
        float shakeTime = Random.Range(0.5f, 3f);
        float shakePower = Random.Range(0.05f, 0.3f);

        CamShake cam = FindObjectOfType<CamShake>();
        cam.VibrateForTime(shakeTime, shakePower);

        float timeRange = Random.Range(5f, 10f);
        Invoke("Shake", timeRange);
    }
}