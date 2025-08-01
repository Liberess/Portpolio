using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElecTrap : MonoBehaviour
{
    private AudioSource myAudio;
    private Transform target;

    private void Awake()
    {
        myAudio = GetComponent<AudioSource>();
    }

    private void Start()
    {
        target = FindObjectOfType<Player>().transform;

        myAudio.volume = 0f;
    }

    private void Update()
    {
        float distance = Vector2.Distance(target.position, transform.position);

        if(distance <= 10f)
        {
            if(myAudio.volume <= 0.7f)
            {
                myAudio.volume += Time.deltaTime * 1.3f;
            }
        }
        else
        {
            if (myAudio.volume >= 0f)
            {
                myAudio.volume -= Time.deltaTime * 2.5f;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.GameOver();
        }
    }
}
