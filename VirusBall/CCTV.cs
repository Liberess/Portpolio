using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCTV : MonoBehaviour
{
    public GameObject mission;

    private void Update()
    {
        if(GameManager.Instance.isGoal)
        {
            mission.SetActive(false);
            GameManager.Instance.isMission = false;
            SoundManager.Instance.PlaySFX("CCTVOff");
            transform.Find("Point Light 2D").gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            mission.SetActive(true);
            collision.GetComponent<Player>().VelocityZero();
            GameManager.Instance.isMission = true;
        }
    }

    /* private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            mission.SetActive(false);
            GameManager.Instance.isMission = false;
        }
    } */
}